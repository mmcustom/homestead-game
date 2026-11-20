// Homestead water (2026-11-19): see-through creek and pond water, clear in the shallows and darker with depth (from the
// scene depth behind the surface), ripples carried downstream by the flow the mesh stores in UV1 (x, z direction,
// z = speed m/s) with two-phase flow mapping so they never stretch, a slower cross ripple from the wind, and foam
// where the water thins against the bank.
float3 posRel = GetCameraRelativePositionWS(PosWS);
float surfaceEye = -TransformWorldToView(posRel).z;
float thick = max(0.0, SceneEye - surfaceEye);
float t = _TimeParameters.x;
float2 flowUV = Flow.xy * Flow.z * FlowSpeed / max(RippleScale, 0.01);
const float period = 3.0;
float ph0 = frac(t / period);
float ph1 = frac(t / period + 0.5);
float w0 = 1.0 - abs(ph0 * 2.0 - 1.0);
float2 uv = PosWS.xz / max(RippleScale, 0.01);
float4 r0 = SAMPLE_TEXTURE2D(Ripple.tex, Ripple.samplerstate, uv - flowUV * ph0 * period);
float4 r1 = SAMPLE_TEXTURE2D(Ripple.tex, Ripple.samplerstate, uv + 0.37 - flowUV * ph1 * period);
float4 r2 = SAMPLE_TEXTURE2D(Ripple.tex, Ripple.samplerstate, uv * 0.41 + float2(t * 0.011, -t * 0.007));
float2 n0 = r0.xy * 2.0 - 1.0, n1 = r1.xy * 2.0 - 1.0, n2 = r2.xy * 2.0 - 1.0;
float2 nxy = (lerp(n1, n0, w0) + n2 * 0.6) * RippleStrength;
NormalWS = normalize(float3(nxy.x, 1.0, nxy.y));
float deep = saturate(thick / max(Clarity, 0.01));
Color = lerp(Shallow.rgb, Deep.rgb, deep);
Alpha = lerp(0.2, 0.93, saturate(thick / max(Clarity * 0.6, 0.01)));
float foamTex = SAMPLE_TEXTURE2D(FoamMap.tex, FoamMap.samplerstate, uv * 1.7 - flowUV * t * 0.25).r;
float edge = saturate(1.0 - thick / max(FoamWidth, 0.001));
float foam = saturate((edge * 1.4 - (1.0 - foamTex)) * 2.0) * step(0.001, thick);
Color = lerp(Color, float3(0.62, 0.63, 0.6), foam * 0.85);
Alpha = max(Alpha, foam * 0.85);
Gloss = lerp(Smooth, 0.35, foam);
