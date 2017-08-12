namespace Rscue.Api.Extensions
{
    using MongoDB.Driver;
    using Rscue.Api.Models;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class MongoDatabaseExtensions
    {
        public static IMongoCollection<Assignment> Assignments(this IMongoDatabase mongoDatabase) => mongoDatabase.GetCollection<Assignment>("assignments");

        public static IMongoCollection<Client> Clients(this IMongoDatabase mongoDatabase) => mongoDatabase.GetCollection<Client>("clients");

        public static IMongoCollection<ProviderWorker> Workers(this IMongoDatabase mongoDatabase) => mongoDatabase.GetCollection<ProviderWorker>("workers");

        public static IMongoCollection<Provider> Providers(this IMongoDatabase mongoDatabase) => mongoDatabase.GetCollection<Provider>("providers");

        public static IMongoCollection<ProviderBoatTow> BoatTows(this IMongoDatabase mongoDatabase) => mongoDatabase.GetCollection<ProviderBoatTow>("boattows");

        public static IMongoCollection<ImageBucket> ImageBuckets(this IMongoDatabase mongoDatabase) => mongoDatabase.GetCollection<ImageBucket>("image-buckets");

        public static UpdateDefinition<TDocument> Set<TDocument>(UpdateDefinition<TDocument> updateDefinition, string fieldName, object value) =>
            (UpdateDefinition<TDocument>)Set(typeof(TDocument), typeof(UpdateDefinitionExtensions), BindingFlags.Static | BindingFlags.Public, updateDefinition, fieldName, value);

        public static UpdateDefinition<TDocument> Set<TDocument>(UpdateDefinitionBuilder<TDocument> updateDefinitionBuilder, string fieldName, object value) =>
            (UpdateDefinition<TDocument>)Set(typeof(TDocument), typeof(UpdateDefinitionBuilder<TDocument>), BindingFlags.Instance | BindingFlags.Public, updateDefinitionBuilder, fieldName, value);

        private static object Set(Type documentType, Type updateDefinitionOrBuilderExtensionsType, BindingFlags bindingFlags, object updateDefinitionOrBuilder, string fieldName, object value)
        {
            var updateDefinitionOrBuilderType = updateDefinitionOrBuilder.GetType();
            var fieldExpression = GetFieldExpression(documentType, fieldName);
            Expression<Func<ProviderWorker, string>> d = _ => _.Name;
            var methods = updateDefinitionOrBuilderExtensionsType.GetMethods(bindingFlags).Where(_ => _.Name == "Set");
            foreach (var m in methods)
            {
                var genericParameters = m.GetGenericArguments();
                if (bindingFlags.HasFlag(BindingFlags.Static))
                {
                    if (genericParameters.Length == 2 && genericParameters[0].Name == "TDocument" && genericParameters[1].Name == "TField")
                    {
                        var genericDocumentType = genericParameters[0];
                        var genericFieldType = genericParameters[1];

                        var expressionType = typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(genericDocumentType, genericFieldType));
                        var mParameters = m.GetParameters();
                        if (mParameters.Length == 3 &&
                            
                            mParameters[1].ParameterType == expressionType &&
                            mParameters[2].ParameterType == genericFieldType)
                        {
                            var mm = m.MakeGenericMethod(documentType, fieldExpression.ReturnType);
                            var fieldExpressionType = typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(documentType, fieldExpression.ReturnType));
                            return mm.Invoke(null, new object[] { updateDefinitionOrBuilder, fieldExpression, value });
                        }
                    }
                }
                else
                {
                    if (genericParameters.Length == 1 && genericParameters[0].Name == "TField")
                    {
                        var genericDocumentType = updateDefinitionOrBuilderType.GetGenericArguments()[0];
                        var genericFieldType = genericParameters[0];

                        var expressionType = typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(genericDocumentType, genericFieldType));
                        var mParameters = m.GetParameters();
                        if (mParameters.Length == 2 &&
                            mParameters[0].ParameterType == expressionType &&
                            mParameters[1].ParameterType == genericFieldType)
                        {
                            var mm = m.MakeGenericMethod(fieldExpression.ReturnType);
                            var fieldExpressionType = typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(documentType, fieldExpression.ReturnType));
                            return mm.Invoke(updateDefinitionOrBuilder, new object[] { fieldExpression, value });
                        }
                    }
                }
            }

            throw new Exception("Method not found");
        }

        private static LambdaExpression GetFieldExpression(Type documentType, string fieldName)
        {
            var propertyInfo = documentType.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            var delegateType = typeof(Func<,>).MakeGenericType(documentType, propertyInfo.PropertyType);
            var param = Expression.Parameter(documentType, "_");
            return Expression.Lambda(delegateType, Expression.PropertyOrField(param, fieldName), param);
        }
    }
}
