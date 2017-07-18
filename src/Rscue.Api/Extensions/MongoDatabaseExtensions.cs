namespace Rscue.Api.Extensions
{
    using MongoDB.Driver;
    using Rscue.Api.Models;
    using System;
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

        public static UpdateDefinition<TDocument> Set<TDocument>(this UpdateDefinition<TDocument> updateDefinition, string fieldName, object value) =>
            (UpdateDefinition<TDocument>)Set(typeof(TDocument), typeof(UpdateDefinition<TDocument>), updateDefinition, fieldName, value);

        public static UpdateDefinition<TDocument> Set<TDocument>(this UpdateDefinitionBuilder<TDocument> updateDefinitionBuilder, string fieldName, object value) =>
            (UpdateDefinition<TDocument>)Set(typeof(TDocument), typeof(UpdateDefinitionBuilder<TDocument>), updateDefinitionBuilder, fieldName, value);

        private static object Set(Type documentType, Type updateDefinitionOrBuilderType, object updateDefinitionOrBuilder, string fieldName, object value)
        {
            var fieldExpression = GetFieldExpression(documentType, fieldName);
            var fieldExpressionType = typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(documentType, fieldExpression.ReturnType));
            var method = typeof(UpdateDefinitionExtensions).GetMethod("Set", new Type[] { updateDefinitionOrBuilderType, fieldExpressionType, fieldExpression.ReturnType });
            return method.Invoke(null, new object[] { updateDefinitionOrBuilder, fieldExpression, value });
        }

        private static LambdaExpression GetFieldExpression(Type documentType, string fieldName)
        {
            var propertyInfo = documentType.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            var delegateType = typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(documentType, propertyInfo.PropertyType));
            return Expression.Lambda(delegateType, Expression.PropertyOrField(Expression.Variable(documentType, "_"), fieldName), Expression.Parameter(documentType, "_"));
        }
    }
}
