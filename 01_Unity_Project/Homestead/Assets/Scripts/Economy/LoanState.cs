using System;
using System.Collections.Generic;

// The land loan's terms, frozen into the loan when it is made so a saved game keeps the terms it started with.
[Serializable]
public struct LoanTerms
{
    public int rateBasisPoints;   // 800 = 8% a year, spread over 12 payments
    public int payments;          // 180
    public int intervalDays;      // a payment falls due every 28 days
    public int firstDueDay;       // TotalDays of the first payment: 112
    public int graceDays;         // unpaid this many days after its due day is late: 10
    public long lateFeeCents;     // $75, once per late payment

    public static LoanTerms Default => new LoanTerms
    {
        rateBasisPoints = 800,
        payments = 180,
        intervalDays = 28,
        firstDueDay = 112,
        graceDays = 10,
        lateFeeCents = 7500,
    };
}

// One payment that has fallen due and is not fully paid. The three amounts are what is still owed on it.
[Serializable]
public class OpenPayment
{
    public int number;       // 1 for the first payment
    public int dueDay;
    public long fee;
    public long interest;
    public long principal;
    public bool missed;      // the late fee has been added
    public int stage;        // the last warning given: 1 due, 2 late, 3 missed

    public long Owed => Money.Add(Money.Add(fee, interest), principal);

    public OpenPayment Clone() => (OpenPayment)MemberwiseClone();
}

public enum LoanNoticeKind { DueSoon, DueToday, Late, Missed }

public struct LoanNotice
{
    public LoanNoticeKind kind;
    public int dueDay;
    public int days;        // DueSoon: days until it is due
    public long amount;     // DueSoon: the regular payment; the others: what that payment owes
}

public struct PayResult
{
    public long applied, toFee, toInterest, toPrincipal, toExtra;
    public int cleared;     // payments paid off by this
    public bool paidOff;    // this payment finished the loan
}

// Money_System.md's land loan, pure and engine-free so edit-mode tests cover it. All amounts are cents (Money); the payment formula
// uses decimal, rounded half up to a cent. The loan is a fixed payment every intervalDays from firstDueDay; each payment's interest
// is the balance at its due date times the period rate, and the rest of the payment is principal. A payment unpaid graceDays after
// its due day gets a one-time late fee and counts as missed. Money pays the oldest open payment first (fee, interest, principal);
// what is left once nothing is open is an extra payment straight to principal, which never covers a future payment.
// Processing is state-based: Advance(today) catches up on every due date and late date up to today, in order, however many days
// passed, and calling it again for the same day does nothing.
[Serializable]
public class LoanState
{
    public int acres;
    public long priceCents;
    public long downCents;
    public long originalCents;
    public long paymentCents;
    public long balanceCents;        // principal still owed, including the principal inside open payments
    public int nextDueDay;           // the next payment to fall due, or -1 when no more will
    public int paymentsDue;          // payments that have fallen due so far
    public int paymentsMade;         // payments paid off in full
    public long interestPaid, feesPaid, principalPaid, extraPaid;
    public int soonNoticeDue = -1;   // the due day the "due soon" warning was last given for
    public LoanTerms terms;
    public List<OpenPayment> open = new List<OpenPayment>();

    public const int SoonDays = 7;

    // --- Formulas ---

    static decimal PeriodRate(LoanTerms t) => t.rateBasisPoints / 10000m / 12m;

    static long RoundCents(decimal cents) => (long)Math.Round(cents, 0, MidpointRounding.AwayFromZero);

    // payment = L*r / (1 - (1+r)^-n), to the cent.
    public static long PaymentFor(long principalCents, LoanTerms t)
    {
        if (principalCents <= 0 || t.payments <= 0)
            return 0;
        decimal r = PeriodRate(t);
        if (r == 0m)
            return RoundCents(principalCents / (decimal)t.payments);

        decimal growth = 1m;
        for (int i = 0; i < t.payments; i++)
            growth *= 1m + r;
        return RoundCents(principalCents * r / (1m - 1m / growth));
    }

    // The interest on a balance for one period.
    public static long InterestFor(long balanceCents, LoanTerms t) => RoundCents(balanceCents * PeriodRate(t));

    // Interest over the rest of a loan paid on schedule with no extra payments: principalCents still to pay, paymentsLeft payments to go.
    public static long TotalInterest(long principalCents, long paymentCents, int paymentsLeft, LoanTerms t)
    {
        long balance = principalCents, total = 0;
        for (int k = 1; balance > 0 && k <= 100000; k++)
        {
            long interest = InterestFor(balance, t);
            long principal = Math.Max(0, paymentCents - interest);
            if (k >= paymentsLeft || principal >= balance)
                principal = balance;
            total = Money.Add(total, interest);
            balance -= principal;
        }
        return total;
    }

    // What a new loan of this size would cost in interest in all, for the New Game summary.
    public static long TotalInterest(long principalCents, LoanTerms t) =>
        TotalInterest(principalCents, PaymentFor(principalCents, t), t.payments, t);

    // --- Making one ---

    public static LoanState Create(int acres, long priceCents, long downCents, LoanTerms terms)
    {
        long down = Math.Max(0, downCents);
        long loan = Math.Max(0, priceCents - down);
        return new LoanState
        {
            acres = acres,
            priceCents = priceCents,
            downCents = down,
            originalCents = loan,
            paymentCents = PaymentFor(loan, terms),
            balanceCents = loan,
            nextDueDay = loan > 0 ? terms.firstDueDay : -1,
            terms = terms,
        };
    }

    // An independent copy (the open payments too), for working out "what if" payments without touching this loan.
    public LoanState Copy()
    {
        Normalize();
        var copy = (LoanState)MemberwiseClone();
        copy.open = new List<OpenPayment>(open.Count);
        foreach (OpenPayment p in open)
            copy.open.Add((OpenPayment)p.Clone());
        return copy;
    }

    // JsonUtility can leave a list null.
    public void Normalize()
    {
        if (open == null)
            open = new List<OpenPayment>();
    }

    // --- What is owed ---

    public long OwedNow
    {
        get
        {
            long total = 0;
            foreach (OpenPayment p in open)
                total = Money.Add(total, p.Owed);
            return total;
        }
    }

    public long FeesOwed
    {
        get
        {
            long total = 0;
            foreach (OpenPayment p in open)
                total = Money.Add(total, p.fee);
            return total;
        }
    }

    public long OpenPrincipal
    {
        get
        {
            long total = 0;
            foreach (OpenPayment p in open)
                total = Money.Add(total, p.principal);
            return total;
        }
    }

    // Principal not yet put into a payment: what future payments and extra payments work on.
    public long UnassignedCents => balanceCents - OpenPrincipal;

    public int MissedCount
    {
        get
        {
            int n = 0;
            foreach (OpenPayment p in open)
                if (p.missed)
                    n++;
            return n;
        }
    }

    public bool IsPaidOff => balanceCents <= 0 && open.Count == 0;

    // The most a payment can apply: everything open, plus every dollar of principal left.
    public long MaxPayableCents => Money.Add(OwedNow, UnassignedCents);

    // Interest still to pay if no extra payment is ever made and every payment is made on time.
    public long InterestRemaining()
    {
        long total = 0;
        foreach (OpenPayment p in open)
            total = Money.Add(total, p.interest);
        long unassigned = UnassignedCents;
        if (unassigned > 0)
            total = Money.Add(total, TotalInterest(unassigned, paymentCents, Math.Max(1, terms.payments - paymentsDue), terms));
        return total;
    }

    // --- Time ---

    // Catches up to the given day (TotalDays): every payment that has fallen due, every late fee, in order. Returns the warnings to give.
    public List<LoanNotice> Advance(int today)
    {
        var notices = new List<LoanNotice>();
        Normalize();

        while (nextDueDay >= 0 && nextDueDay <= today)
            GeneratePayment(nextDueDay);

        foreach (OpenPayment p in open)
        {
            if (!p.missed && today >= p.dueDay + terms.graceDays)
            {
                p.missed = true;
                p.fee = Money.Add(p.fee, terms.lateFeeCents);
            }

            int target = p.missed ? 3 : today > p.dueDay ? 2 : today == p.dueDay ? 1 : 0;
            if (target > p.stage)
            {
                p.stage = target;
                LoanNoticeKind kind = target == 1 ? LoanNoticeKind.DueToday : target == 2 ? LoanNoticeKind.Late : LoanNoticeKind.Missed;
                notices.Add(new LoanNotice { kind = kind, dueDay = p.dueDay, amount = p.Owed });
            }
        }

        if (nextDueDay > today && nextDueDay - today <= SoonDays && soonNoticeDue != nextDueDay)
        {
            soonNoticeDue = nextDueDay;
            notices.Add(new LoanNotice { kind = LoanNoticeKind.DueSoon, dueDay = nextDueDay, days = nextDueDay - today, amount = paymentCents });
        }
        return notices;
    }

    void GeneratePayment(int dueDay)
    {
        long unassigned = UnassignedCents;
        if (unassigned <= 0)
        {
            nextDueDay = -1;
            return;
        }

        paymentsDue++;
        long interest = InterestFor(balanceCents, terms);
        long principal = Math.Max(0, paymentCents - interest);
        bool last = principal >= unassigned || paymentsDue >= terms.payments;
        if (last)
            principal = unassigned; // the last payment clears what is left exactly

        open.Add(new OpenPayment { number = paymentsDue, dueDay = dueDay, interest = interest, principal = principal });
        nextDueDay = last ? -1 : dueDay + terms.intervalDays;
    }

    // --- Paying ---

    // Pays up to amountCents, never more than cashCents or than the loan can take. The caller takes the applied amount out of cash.
    public PayResult Pay(long cashCents, long amountCents)
    {
        var result = new PayResult();
        Normalize();
        long amount = Math.Min(Math.Min(amountCents, cashCents), MaxPayableCents);
        if (amount <= 0)
            return result;

        bool wasPaidOff = IsPaidOff;
        long left = amount;
        int index = 0;
        while (index < open.Count && left > 0)
        {
            OpenPayment p = open[index];
            long toFee = Math.Min(left, p.fee);
            p.fee -= toFee;
            left -= toFee;
            long toInterest = Math.Min(left, p.interest);
            p.interest -= toInterest;
            left -= toInterest;
            long toPrincipal = Math.Min(left, p.principal);
            p.principal -= toPrincipal;
            left -= toPrincipal;

            result.toFee += toFee;
            result.toInterest += toInterest;
            result.toPrincipal += toPrincipal;
            feesPaid += toFee;
            interestPaid += toInterest;
            principalPaid += toPrincipal;
            balanceCents -= toPrincipal;

            if (p.Owed == 0)
            {
                open.RemoveAt(index);
                paymentsMade++;
                result.cleared++;
            }
            else
            {
                index++;
            }
        }

        // Anything left with nothing open is an extra payment, straight to principal.
        long extra = Math.Min(left, UnassignedCents);
        if (extra > 0)
        {
            balanceCents -= extra;
            extraPaid += extra;
            result.toExtra = extra;
            left -= extra;
        }

        if (UnassignedCents <= 0)
            nextDueDay = -1;

        result.applied = amount - left;
        result.paidOff = !wasPaidOff && IsPaidOff;
        return result;
    }
}

// The loan in a save (player file, key "loan"). A save with no such section, or hasLoan false, has no loan at all.
[Serializable]
public struct LoanSaveData
{
    public const int CurrentVersion = 1;

    public int version;
    public bool hasLoan;
    public LoanState loan;

    public static LoanSaveData Of(LoanState loan) => new LoanSaveData
    {
        version = CurrentVersion,
        hasLoan = loan != null,
        loan = loan,
    };

    // The loan to restore, or null when the save has none.
    public LoanState ToLoan()
    {
        if (!hasLoan || loan == null)
            return null;
        loan.Normalize();
        return loan;
    }
}
