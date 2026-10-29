using System;
using MadSlime.ShapeFill.Logic;
using MadSlime.Validation;

namespace MadSlime.ShapeFill.FillStateMachine.Logic
{
    public class FillSessionData
    {
        public FillResult InitialResult { get; private set; }

        public bool IsRescueUsed { get; private set; }

        public FillStartMode StartMode { get; private set; }

        public void Prepare(FillResult initialResult)
        {
            ArgumentGuard.CheckNotNull(initialResult, nameof(initialResult));

            InitialResult = initialResult;
            IsRescueUsed = false;
            StartMode = FillStartMode.Initial;
        }

        public void MarkRescueUsed()
        {
            if (InitialResult == null)
            {
                throw new InvalidOperationException("Cannot use rescue before the fill session is prepared.");
            }

            if (IsRescueUsed)
            {
                throw new InvalidOperationException("Rescue has already been used in this fill session.");
            }

            if (InitialResult.IsWin)
            {
                throw new InvalidOperationException("Cannot use rescue for a winning fill result.");
            }

            IsRescueUsed = true;
        }

        public void PrepareRescueContinuation()
        {
            if (InitialResult == null)
            {
                throw new InvalidOperationException(
                    "Cannot prepare rescue continuation before the fill session is prepared.");
            }

            if (IsRescueUsed == false)
            {
                throw new InvalidOperationException("Rescue must be marked as used before preparing its continuation.");
            }

            if (StartMode == FillStartMode.AfterRescue)
            {
                throw new InvalidOperationException(
                    "Rescue continuation has already been prepared for this fill session.");
            }

            StartMode = FillStartMode.AfterRescue;
        }
    }
}
