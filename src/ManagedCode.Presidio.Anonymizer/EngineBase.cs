namespace ManagedCode.Presidio.Anonymizer;

/// <summary>
/// Handles the logic of operations over text using registered operators.
/// </summary>
public abstract class EngineBase
{
    protected EngineBase()
    {
        OperatorsFactory = new OperatorsFactory();
    }

    protected OperatorsFactory OperatorsFactory { get; }

    protected EngineResult Operate(
        string text,
        IReadOnlyCollection<PiiEntity> piiEntities,
        IDictionary<string, OperatorConfig> operatorsMetadata,
        OperatorType operatorType,
        IDictionary<string, object?>? operatorArguments = null)
    {
        ArgumentNullException.ThrowIfNull(piiEntities);
        ArgumentNullException.ThrowIfNull(operatorsMetadata);

        var textReplaceBuilder = new TextReplaceBuilder(text);
        var engineResult = new EngineResult();
        var sortedEntities = piiEntities
            .OrderByDescending(entity => entity.Start)
            .ThenByDescending(entity => entity.End)
            .ToList();

        foreach (var entity in sortedEntities)
        {
            var textToOperateOn = textReplaceBuilder.GetTextInPosition(entity.Start, entity.End);
            var operatorConfig = GetEntityOperatorMetadata(entity.EntityType, operatorsMetadata);
            var changedText = OperateOnText(entity, textToOperateOn, operatorConfig, operatorType, operatorArguments);
            var indexFromEnd = textReplaceBuilder.ReplaceTextGetInsertionIndex(changedText, entity.Start, entity.End);

            var resultItem = new OperatorResult(0, indexFromEnd, entity.EntityType, changedText, operatorConfig.OperatorName);
            engineResult.AddItem(resultItem);
        }

        engineResult.SetText(textReplaceBuilder.OutputText);
        engineResult.NormalizeItemIndexes();
        return engineResult;
    }

    private static OperatorConfig GetEntityOperatorMetadata(string entityType, IDictionary<string, OperatorConfig> operatorsMetadata)
    {
        if (operatorsMetadata.TryGetValue(entityType, out var config))
        {
            return config;
        }

        if (operatorsMetadata.TryGetValue("DEFAULT", out var defaultConfig))
        {
            return defaultConfig;
        }

        throw new InvalidParamException($"Operator configuration missing DEFAULT entry for entity '{entityType}'.");
    }

    private string OperateOnText(
        PiiEntity entity,
        string textToOperateOn,
        OperatorConfig operatorConfig,
        OperatorType operatorType,
        IDictionary<string, object?>? operatorArguments)
    {
        var parameters = new Dictionary<string, object?>(operatorConfig.Parameters, StringComparer.Ordinal)
        {
            ["entity_type"] = entity.EntityType,
        };

        if (operatorArguments is not null)
        {
            foreach (var argument in operatorArguments)
            {
                parameters[argument.Key] = argument.Value;
            }
        }

        var @operator = OperatorsFactory.CreateOperator(operatorConfig.OperatorName, operatorType);
        @operator.Validate(parameters);
        return @operator.Operate(textToOperateOn, parameters);
    }
}
