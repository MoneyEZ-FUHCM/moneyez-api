using MoneyEz.Services.BusinessModels.FinancialGoalModels;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IGoalPredictionService
    {
        Task<GoalPredictionModel> PredictGoalCompletion(Guid goalId, bool isSaving);
    }
}
