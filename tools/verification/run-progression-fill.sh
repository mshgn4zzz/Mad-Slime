#!/usr/bin/env bash
set -euo pipefail
root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
editor="${UNITY_EDITOR_PATH:-/home/mshgn4zzz/Unity/Hub/Editor/2022.3.62f2/Editor/Unity}"
mode="${1:-checks}"
case "$mode" in checks|animation|render|all) ;; *) echo 'Usage: run-progression-fill.sh [checks|animation|render|all]' >&2; exit 2;; esac
[[ -x "$editor" ]] || { echo 'Set UNITY_EDITOR_PATH to the Unity 2022.3 executable.' >&2; exit 2; }
# A separate project keeps the open Editor, its scene and player save untouched.
if [[ -n "${UNITY_VERIFICATION_PROJECT:-}" ]]; then
    verification_dir="$(realpath -- "$UNITY_VERIFICATION_PROJECT")"
    [[ "$verification_dir" != "$root" && "$verification_dir" != / && -d "$verification_dir/ProjectSettings" ]] || exit 2
else
    verification_dir="$(mktemp -d "$(dirname -- "$root")/MadSlimeProgressionFill.XXXXXX")"
    trap 'rm -rf -- "$verification_dir"' EXIT
fi
cp -a --reflink=auto "$root/Assets" "$root/Packages" "$root/ProjectSettings" "$verification_dir/"
mkdir -p "$verification_dir/Assets/MadSlimeVerification/Editor" "$root/output/progression-fill"
cp "$root"/tools/verification/*Checks.cs "$verification_dir/Assets/MadSlimeVerification/Editor/"
for task in checks animation render; do
    [[ "$mode" == all || "$mode" == "$task" ]] || continue
    execute="ProgressionFillChecks.Run"
    [[ "$task" != animation ]] || execute="FillAnimationChecks.Run"
    [[ "$task" != render ]] || execute="ProgressionFillChecks.Render"
    flags=(-batchmode)
    [[ "$task" == render ]] || flags+=(-nographics)
    result=0
    timeout 900 "$editor" "${flags[@]}" -projectPath "$verification_dir" -executeMethod "$execute" -logFile "$root/output/progression-fill/$task.log" || result=$?
    if [[ -d "$verification_dir/output/progression-fill" ]]; then
        cp -a "$verification_dir/output/progression-fill/." "$root/output/progression-fill/"
    fi
    [[ "$result" == 0 ]] || exit "$result"
done
[[ ! -f "$root/output/progression-fill/checks.txt" ]] || cat "$root/output/progression-fill/checks.txt"
[[ ! -f "$root/output/progression-fill/animation.txt" ]] || cat "$root/output/progression-fill/animation.txt"
