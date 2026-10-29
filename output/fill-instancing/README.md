# Fill GPU instancing — 2026-10-29

Changed `FillCubeUnlit.shader` to compile instancing variants, set up instance IDs for object transforms and fragment color, and read `_Color` from an instancing buffer. Enabled instancing on `FlyingCubePlastic.mat`. Existing CubeSpawner MaterialPropertyBlock coloring is retained.

## Measured result

Unity 2022.3.62f2; NVIDIA RTX 4060; OpenGLCore; 1024 × 768.
Actual scene flow: Menu → Game → Fill. Filled the current shape using the real ShapeFiller and acceleration path. Gameplay, UI, player and scene visuals remained enabled. 1,016 FlyingCube objects. Waited for completion and settling before collecting 60 distinct rendered frames in each condition.

| Material GPU instancing | Mean batches | Min | Max | Frames |
| --- | ---: | ---: | ---: | ---: |
| Disabled | 1029 | 1029 | 1029 | 60 |
| Enabled | 15 | 15 | 15 | 60 |

Both conditions use the updated shader, differing only in Material.enableInstancing. The original shader has no instanced variant; the disabled condition reproduces the separate-renderer path.

For rendering in batch-mode Play Mode, the test assigns the main camera a RenderTexture and routes existing root overlay canvases through that camera. These test-only settings are applied only in the disposable project, never saved to the source scene. Thus these numbers are an offscreen Editor rendering comparison including UI, not browser measurements.

The before/after PNGs retain identical pixels outside the animated player bounds (x=427..581, y=581..730). The picture itself is pixel-identical, confirming per-cube colors and shading are preserved. The harness completed with exit code 0; the final log has no C# compilation errors, shader errors, or runtime exceptions. DOTween emitted existing automatic tween-capacity expansion warnings during instant fill.

## Scope

This verifies the completed scene in Editor/OpenGLCore. It does not establish a maximum of 50 across all levels, transient effects, flight animations, devices, or a WebGL browser build. It does not measure GPU/CPU frame time. No mesh merging or pooling is added.

## Files

- `full-scene.txt`: raw recorded values.
- `full-scene-before.png`, `full-scene-after.png`: camera output including UI.
- `full-scene-editor.log`: final Unity run log.
- `FillSceneBatchMeasurement.cs`: test-only reproduction harness, kept outside Assets. For reuse, copy it to an Editor folder in a disposable project and run `-batchmode -executeMethod FillSceneBatchMeasurement.Run` without `-quit`; the harness exits itself.
