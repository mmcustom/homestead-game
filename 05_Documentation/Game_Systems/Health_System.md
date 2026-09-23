# Homestead
## Health System v1.0

Status: Design Draft

---

# Purpose

The Health System represents the player's overall physical condition.

Health is not the same as hunger, hydration, or fatigue.

Those systems influence health.

Health is the final measure of a player's ability to survive.

---

# Core Philosophy

Players rarely die from a single mistake.

Most health problems result from:

- Poor planning
- Ignoring warning signs
- Repeated bad decisions

Examples:

Unsafe Water
↓
Illness
↓
Dehydration
↓
Health Loss

---

# Health Attribute

Range:

0-100

Health changes slowly.

Most actions affect other systems first.

Those systems then affect health.

---

# Health States

## 75-100

Healthy

No penalties.

---

## 50-74

Minor Health Issues

Effects:

- Reduced stamina recovery
- Increased fatigue accumulation

---

## 25-49

Poor Health

Effects:

- Slower movement
- Reduced work efficiency
- Faster fatigue accumulation

---

## 1-24

Critical Health

Effects:

- Severe movement penalties
- Risk of collapse

---

## 0

Player Collapse

Player becomes incapacitated.

Final implementation TBD.

---

# Illness System

Illness is primarily connected to water quality.

---

## Mild Illness

Causes:

- Questionable water
- Poor nutrition

Effects:

- Increased thirst
- Reduced energy

---

## Moderate Illness

Effects:

- Significant dehydration
- Reduced productivity

---

## Severe Illness

Effects:

- Large health reduction
- Extended recovery requirements

---

# Exposure System

Exposure occurs when environmental conditions exceed shelter protection.

Examples:

- Extreme cold
- Storms
- Long-term wet conditions

---

## Mild Exposure

Effects:

- Increased fatigue

---

## Moderate Exposure

Effects:

- Health loss begins

---

## Severe Exposure

Effects:

- Rapid health decline

---

# Recovery

Health recovers through:

- Clean water
- Quality food
- Adequate sleep
- Shelter

Recovery should be gradual.

---

# Nutrition

Future Expansion

Health benefits from balanced diets.

Food Categories:

- Meat
- Fish
- Fruits
- Nuts
- Vegetables

Balanced nutrition improves long-term resilience.

---

# Morale

Morale represents the player's motivation and outlook.

Range:

0-100

---

## Morale Increases

Examples:

- Good meals
- Warm shelter
- Successful hunts
- Completed projects

---

## Morale Decreases

Examples:

- Hunger
- Illness
- Lack of sleep
- Overwork / Lack of rest
- Storm damage
- Livestock loss

---

# Morale Effects

High Morale

Benefits:

- Small work efficiency bonus
- Slight recovery bonus

Low Morale

Effects:

- Reduced productivity
- Faster fatigue accumulation

Morale should matter but never dominate gameplay.

---

# Design Rules

1. Health declines gradually.

2. Small problems become major problems if ignored.

3. Recovery requires preparation.

4. Clean water is the most important health factor.

5. Health supports realism without becoming overly punitive.