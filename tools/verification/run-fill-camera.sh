#!/usr/bin/env bash
set -euo pipefail
root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
editor="${UNITY_EDITOR_PATH:-/home/mshgn4zzz/Unity/Hub/Editor/2022.3.62f2/Editor/Unity}"
mode="${1:-all}"
case "$mode" in checks|render|live|all) ;; *) echo 'Usage: run-fill-camera.sh [checks|render|live|all]' >&2; exit 2;; esac
[[ -x "$editor" ]] || { echo 'Set UNITY_EDITOR_PATH to Unity 2022.3.' >&2; exit 2; }
# A separate project keeps the open Editor, its scene and player save untouched.
if [[ -n "${UNITY_VERIFICATION_PROJECT:-}" ]]; then
    verification_dir="$(realpath -- "$UNITY_VERIFICATION_PROJECT")"
    [[ "$verification_dir" != "$root" && "$verification_dir" != / && -d "$verification_dir/ProjectSettings" ]] || exit 2
else
    verification_dir="$(mktemp -d "$(dirname -- "$root")/MadSlimeCameraCheck.XXXXXX")"
    trap 'rm -rf -- "$verification_dir"' EXIT
fi
cp -a --reflink=auto "$root/Assets" "$root/Packages" "$root/ProjectSettings" "$verification_dir/"
mkdir -p "$verification_dir/Assets/CameraVerification/Editor" "$root/output/camera-fit"
cp "$root/tools/verification/FillCameraChecks.cs" "$verification_dir/Assets/CameraVerification/Editor/"
for task in checks render live; do
    [[ "$mode" == all || "$mode" == "$task" ]] || continue
    method=Run
    [[ "$task" != render ]] || method=Render
    [[ "$task" != live ]] || method=Live
    flags=(-batchmode)
    [[ "$task" == render ]] || flags+=(-nographics)
    result=0
    timeout 300 "$editor" "${flags[@]}" -projectPath "$verification_dir" -executeMethod "FillCameraChecks.$method" -logFile "$root/output/camera-fit/$task.log" || result=$?
    if [[ -d "$verification_dir/output/camera-fit" ]]; then
        cp -a "$verification_dir/output/camera-fit/." "$root/output/camera-fit/"
    fi
    [[ "$result" == 0 ]] || exit "$result"
done
[[ ! -f "$root/output/camera-fit/checks.txt" ]] || head -1 "$root/output/camera-fit/checks.txt"
[[ ! -f "$root/output/camera-fit/live.txt" ]] || cat "$root/output/camera-fit/live.txt"
