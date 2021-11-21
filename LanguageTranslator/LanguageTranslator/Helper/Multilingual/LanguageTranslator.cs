//---------------------------------------------------------------------
// <copyright file="LanguageTranslator.cs" owner="MobinAli">
// </copyright>
//---------------------------------------------------------------------
namespace LanguageTranslator.Helper.Multilingual
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using LanguageTranslator.Models;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Unity;


    /// <summary>
    /// Translator.
    /// </summary>
    /// <typeparam name="T">Generic type.</typeparam>
    public class LanguageTranslator<T>
    {
        /// <summary>
        /// URL of the Language Translation Service.
        /// </summary>
        private static string TranslationServiceUrl;

        /// <summary>
        /// Headers of the Language Translation Service.
        /// </summary>
        private static Dictionary<string, string> Headers;

        //  private static readonly ITelemetry Logger;
        private Dictionary<string, string> translatableItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageTranslator{T}"/> class.
        /// </summary>
        /// <param name="entityToTranslate">Entity to translate.</param>
        /// <param name="entityName">Name of the entity to translate.</param>
        /// <param name="propertiesToTranslate">Optional: Names of the properties to translate.</param>
        /// <param name="propertiesToIgnore">Optional: Names of the properties to ignore.</param>
        public LanguageTranslator(T entityToTranslate, IConfiguration configuration, string entityName = null, List<string> propertiesToTranslate = null, List<string> propertiesToIgnore = null)
        {
            TranslationServiceUrl = configuration.GetValue<string>("BaseAddress");

            Headers = new Dictionary<string, string>()
            {
                { "Accept", configuration.GetValue<string>("ApiVersion") }
            };

            this.PropertiesToTranslate = new List<string>();

            if (propertiesToTranslate != null && propertiesToTranslate.Count > 0)
            {
                this.PropertiesToTranslate = propertiesToTranslate;
            }

            this.PropertiesToIgnore = new List<string>();

            if (propertiesToIgnore != null && propertiesToIgnore.Count > 0)
            {
                this.PropertiesToIgnore = propertiesToIgnore;
            }

            if (this.IsNull(entityToTranslate))
            {
                this.EntityToTranslate = default(T);
            }
            else
            {
                this.EntityToTranslate = entityToTranslate;
            }

            if (string.IsNullOrEmpty(entityName))
            {
                this.EntityName = nameof(this.EntityToTranslate);
            }
            else
            {
                this.EntityName = entityName;
            }
        }

        /// <summary>
        /// Gets or sets the entity to translate.
        /// </summary>
        public T EntityToTranslate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the entity to translate.
        /// </summary>
        public string EntityName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the property names to translate.
        /// </summary>
        public List<string> PropertiesToTranslate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the property names to ignore.
        /// </summary>
        public List<string> PropertiesToIgnore
        {
            get;
            set;
        }

        /// <summary>
        /// Indexer for translatable items.
        /// </summary>
        /// <param name="key">Property name.</param>
        /// <returns>Property value.</returns>
        private object this[string key]
        {
            get
            {
                return this.translatableItems[key];
            }

            set
            {
                if (!string.IsNullOrEmpty(key) && !this.translatableItems.ContainsKey(key) && value != null)
                {
                    string valueAsString = value.ToString();

                    if (!string.IsNullOrEmpty(valueAsString))
                    {
                        this.translatableItems.Add(key, valueAsString);
                    }
                }
            }
        }

        /// <summary>
        /// Translate.
        /// </summary>
        /// <param name="language">Language to translate to.</param>
        /// <returns>Entity to translate.</returns>
        public async Task<T> TranslateAsync(Language language)
        {
            if (this.IsNull(this.EntityToTranslate) || language == Language.English)
            {
                return this.EntityToTranslate;
            }

            T translatedEntity = default(T);

            try
            {
                this.translatableItems = new Dictionary<string, string>();

                // If properties to translate contains nested properties, we extract and add the parent properties.
                // We aren't using this logic with properties to ignore because they are to be matched exactly.
                this.AddParentPropertiesIfAny();

                translatedEntity = DeepCopyHelper.Clone<T>(this.EntityToTranslate);

                this.TranslateRecursively(translatedEntity, this.EntityName, translatedEntity.GetType());

                TranslationMessage translationMessage = new TranslationMessage
                {
                    To = LanguageHelper.GetLanguageName(language),
                    Translations = this.translatableItems
                };

                if (translationMessage.Translations.Count > 0)
                {
                    string httpRequestContent = JsonConvert.SerializeObject(translationMessage);
                    string httpResponseContent = await this.CallTranslationAPIAsync(httpRequestContent);

                    if (!string.IsNullOrEmpty(httpResponseContent))
                    {
                        TranslationMessage translatedMessage = JsonConvert.DeserializeObject<TranslationMessage>(httpResponseContent);

                        translatedEntity = this.Map(translatedEntity, translatedMessage.Translations);
                    }
                }
            }
            catch (Exception ex)
            {
                translatedEntity = this.EntityToTranslate;
            }

            return translatedEntity;
        }

        private void AddParentPropertiesIfAny()
        {
            List<string> parentProperties = new List<string>();

            this.PropertiesToTranslate.ForEach(propertyToTranslate =>
            {
                if (propertyToTranslate.Contains("."))
                {
                    // There will always be two or more parts.
                    string[] propertyToTranslateParts = propertyToTranslate.Split(new char[] { '.' });

                    StringBuilder propertyName = new StringBuilder();

                    for (int j = 0; j <= propertyToTranslateParts.Length - 2; j++)
                    {
                        propertyName.Append(string.Format("{0}.", propertyToTranslateParts[j]));

                        parentProperties.Add(propertyName.ToString().TrimEnd(new char[] { '.' }));
                    }
                }
            });

            parentProperties.ForEach(parentProperty =>
            {
                if (!this.PropertiesToTranslate.Contains(parentProperty))
                {
                    this.PropertiesToTranslate.Add(parentProperty);
                }
            });
        }

        /// <summary>
        /// Calls  Language Translation Service.
        /// </summary>
        /// <param name="httpRequestContent">An instance of the <see cref="TranslationMessage"/> class serialized as a string.</param>
        /// <returns>A string representing an instance of the <see cref="TranslationMessage"/> class.</returns>
        private async Task<string> CallTranslationAPIAsync(string httpRequestContent)
        {
            if (string.IsNullOrEmpty(httpRequestContent))
            {
                return string.Empty;
            }

            using (var httpClient = new HttpClient())
            {
                using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(TranslationServiceUrl)))
                {
                    foreach (KeyValuePair<string, string> header in Headers)
                    {
                        httpRequestMessage.Headers.Add(header.Key, header.Value);
                    }

                    httpRequestMessage.Content = new StringContent(httpRequestContent, Encoding.UTF8, "application/json");

                    HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

                    if (httpResponseMessage == null && httpResponseMessage.Content == null)
                    {
                        return string.Empty;
                    }

                    string httpResponseMessageString = await httpResponseMessage.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(httpResponseMessageString))
                    {
                        return string.Empty;
                    }

                    JToken jToken = JToken.Parse(httpResponseMessageString);

                    if (jToken == null)
                    {
                        return string.Empty;
                    }

                    return jToken.ToString(Formatting.Indented);
                }
            }
        }

        private T Map(T entityToMap, Dictionary<string, string> translations)
        {
            foreach (KeyValuePair<string, string> keyValuePair in translations)
            {
                // Remove first level of nesting.
                string propertyName = keyValuePair.Key.Replace(string.Format("{0}.", this.EntityName), string.Empty);

                if (string.IsNullOrEmpty(propertyName) || propertyName.Equals(nameof(this.EntityToTranslate)))
                {
                    entityToMap = (T)Convert.ChangeType(keyValuePair.Value, typeof(T));
                }
                else if (propertyName.Contains("."))
                {
                    this.SetNestedPropertyValue(entityToMap, propertyName, keyValuePair.Value);
                }
                else
                {
                    this.SetPropertyValue(entityToMap, propertyName, keyValuePair.Value);
                }
            }

            return entityToMap;
        }

        private void SetNestedPropertyValue(dynamic entity, string propertyName, object propertyValue)
        {
            Type entityType = entity.GetType();
            string[] propertyNames = null;

            if (propertyName.Contains("[") || propertyName.Contains("]"))
            {
                propertyNames = this.GetPropertyNamesBySplitting(propertyName);
            }
            else
            {
                propertyNames = propertyName.Split('.');
            }

            for (int i = 0; i < propertyNames.Length - 1; i++)
            {
                object index = null;

                if (propertyNames[i].Contains("[") || propertyNames[i].Contains("]"))
                {
                    string indexWithBrackets = this.GetIndexWithBrackets(propertyNames[i]);

                    index = indexWithBrackets.Trim(new char[] { '[', ']' });

                    propertyNames[i] = propertyNames[i].Replace(indexWithBrackets, string.Empty);
                }

                this.LoadPropertyValue(ref entity, propertyNames[i]);

                if (index != null)
                {
                    if (entityType.Name.ToLower().Contains("dictionary"))
                    {
                        Type typeOfKey = entityType.GetGenericArguments()[0];

                        this.LoadItem(ref entity, Convert.ChangeType(index, typeOfKey));
                    }
                    else
                    {
                        try
                        {
                            this.LoadItem(ref entity, Convert.ToInt32(index));
                            continue;
                        }
                        catch
                        {
                        }

                        try
                        {
                            this.LoadItem(ref entity, Convert.ToString(index));
                            continue;
                        }
                        catch
                        {
                        }

                        try
                        {
                            this.LoadItem(ref entity, Convert.ToDateTime(index));
                            continue;
                        }
                        catch
                        {
                        }

                        try
                        {
                            this.LoadItem(ref entity, index);
                            continue;
                        }
                        catch
                        {
                        }
                    }
                }
            }

            string lastPropertyName = propertyNames.Last();

            if (lastPropertyName.Contains("[") || lastPropertyName.Contains("]"))
            {
                string indexWithBrackets = this.GetIndexWithBrackets(lastPropertyName);

                this.LoadPropertyValue(ref entity, lastPropertyName.Replace(indexWithBrackets, string.Empty));
            }

            this.SetPropertyValue(entity, lastPropertyName, propertyValue);
        }

        private string[] GetPropertyNamesBySplitting(string propertyName)
        {
            List<string> propertyNames = new List<string>();
            StringBuilder temporaryName = new StringBuilder();
            bool squareBracketStartFound = false;

            for (int i = 0; i < propertyName.Length; i++)
            {
                if (propertyName[i] == '.')
                {
                    if (squareBracketStartFound)
                    {
                        temporaryName.Append(propertyName[i]);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(temporaryName.ToString()))
                        {
                            propertyNames.Add(temporaryName.ToString());

                            temporaryName = new StringBuilder();
                        }
                    }
                }
                else if (propertyName[i] == '[')
                {
                    squareBracketStartFound = true;

                    temporaryName.Append(propertyName[i]);
                }
                else if (propertyName[i] == ']')
                {
                    squareBracketStartFound = false;

                    temporaryName.Append(propertyName[i]);

                    propertyNames.Add(temporaryName.ToString());

                    temporaryName = new StringBuilder();
                }
                else
                {
                    temporaryName.Append(propertyName[i]);
                }
            }

            if (!string.IsNullOrEmpty(temporaryName.ToString()))
            {
                propertyNames.Add(temporaryName.ToString());

                temporaryName = new StringBuilder();
            }

            return propertyNames.ToArray();
        }

        private void LoadItem(ref dynamic entity, object index)
        {
            PropertyInfo propertyInfo = entity.GetType().GetProperty("Item");

            if (propertyInfo != null)
            {
                entity = propertyInfo.GetValue(entity, new object[] { index });
            }
        }

        private void LoadPropertyValue(ref dynamic entity, string propertyName)
        {
            PropertyInfo propertyInfo = entity.GetType().GetProperty(propertyName);

            if (propertyInfo != null)
            {
                entity = propertyInfo.GetValue(entity);
            }
        }

        private void SetPropertyValue(dynamic entity, string propertyName, object propertyValue)
        {
            PropertyInfo propertyToSet = null;
            Type entityType = entity.GetType();

            if (propertyName.Contains("[") || propertyName.Contains("]"))
            {
                string indexWithBrackets = this.GetIndexWithBrackets(propertyName);

                object index = indexWithBrackets.Trim(new char[] { '[', ']' });

                propertyToSet = entityType.GetProperty("Item");

                if (propertyToSet != null && propertyToSet.CanWrite)
                {
                    if (entityType.Name.ToLower().Contains("dictionary"))
                    {
                        Type typeOfKey = entityType.GetGenericArguments()[0];

                        propertyToSet.SetValue(entity, propertyValue, new object[] { Convert.ChangeType(index, typeOfKey) });
                    }
                    else
                    {
                        propertyToSet.SetValue(entity, propertyValue, new object[] { Convert.ToInt32(index) });
                    }
                }
            }
            else
            {
                propertyToSet = entityType.GetProperty(propertyName);

                if (propertyToSet != null && propertyToSet.CanWrite)
                {
                    propertyToSet.SetValue(entity, propertyValue);
                }
            }
        }

        private void TranslateRecursively(object objectToRecurse, string objectName, Type objectType, List<Type> parentTypes = null)
        {
            if (objectToRecurse == null || string.IsNullOrEmpty(objectName) || this.IsTypeNonTranslatable(objectType) || !this.IsPropertyTranslatable(objectName))
            {
                return;
            }

            if (objectType.Equals(typeof(string)))
            {
                this.MarkAsTranslatableItem(objectToRecurse, objectName);
            }
            else if (objectType.IsArray || objectType.Namespace.ToLower().Contains("collection") || objectType.IsGenericType)
            {
                this.TranslateCollection(objectToRecurse, objectName, objectType, parentTypes);
            }
            else if (objectType.IsClass)
            {
                this.TranslateClass(objectToRecurse, objectName, objectType, parentTypes);
            }
        }

        private void TranslateClass(object objectToRecurse, string objectName, Type objectType, List<Type> parentTypes = null)
        {
            List<PropertyInfo> properties = this.GetShortListedProperties(objectType);

            foreach (PropertyInfo property in this.YieldPropertyInfo(properties))
            {
                string propertyName = this.GetFullyQualifiedNameOfProperty(property, objectName);

                object propertyValue = null;
                Type propertyType = property.PropertyType;

                if (propertyType.Equals(typeof(string)))
                {
                    propertyValue = this.GetNestedPropertyValue(properties, objectToRecurse, propertyName);
                }
                else if (propertyType.IsArray || propertyType.Namespace.ToLower().Contains("collection") || propertyType.IsGenericType)
                {
                    this.AddTypeIfNotExists(ref parentTypes, objectType);

                    propertyValue = property.GetValue(objectToRecurse);
                }
                else if (propertyType.IsClass)
                {
                    if (parentTypes != null && parentTypes.Contains(propertyType))
                    {
                        continue;
                    }

                    this.AddTypeIfNotExists(ref parentTypes, objectType);

                    propertyValue = property.GetValue(objectToRecurse);
                }

                this.TranslateRecursively(propertyValue, propertyName, propertyType, parentTypes);
            }
        }

        private void TranslateCollection(object objectToRecurse, string objectName, Type objectType, List<Type> parentTypes = null)
        {
            Type[] typesOfItemsInCollection = null;

            if (objectType.IsArray)
            {
                typesOfItemsInCollection = new Type[] { objectType.GetElementType() };
            }
            else if (objectType.Namespace.ToLower().Contains("collection") || objectType.IsGenericType)
            {
                if (objectType.Name.ToLower().Contains("arraylist"))
                {
                    typesOfItemsInCollection = new Type[1] { typeof(object) };
                }
                else
                {
                    typesOfItemsInCollection = objectType.GetGenericArguments();
                }
            }
            else
            {
                return;
            }

            if (typesOfItemsInCollection.All(x => this.IsTypeNonTranslatable(x)))
            {
                return;
            }

            dynamic actualCollectionObject = objectToRecurse;

            int collectionSize = this.GetCollectionSize(actualCollectionObject);

            if (collectionSize == 0)
            {
                return;
            }

            int? index = default(int?);

            if (!objectType.Name.ToLower().Contains("dictionary"))
            {
                index = 0;
            }

            if (index.HasValue)
            {
                for (int i = 0; i < collectionSize; i++)
                {
                    var itemInCollection = actualCollectionObject[i];

                    if (itemInCollection != null)
                    {
                        this.TranslateRecursively(itemInCollection, string.Format("{0}[{1}]", objectName, i), itemInCollection.GetType(), parentTypes);
                    }
                }
            }
            else
            {
                dynamic collectionEnumerator = objectType.GetMethod("GetEnumerator").Invoke(actualCollectionObject, null);

                while (collectionEnumerator.MoveNext())
                {
                    var itemInCollection = collectionEnumerator.Current;

                    if (itemInCollection.Value != null)
                    {
                        this.TranslateRecursively(itemInCollection.Value, string.Format("{0}[{1}]", objectName, itemInCollection.Key), itemInCollection.Value.GetType(), parentTypes);
                    }
                }
            }
        }

        private void MarkAsTranslatableItem(object objectToRecurse, string objectName)
        {
            if (!string.IsNullOrEmpty(objectToRecurse.ToString()))
            {
                this[objectName] = objectToRecurse;
            }
        }

        private object GetNestedPropertyValue(List<PropertyInfo> properties, object actualObject, string propertyName)
        {
            if (properties == null || actualObject == null || string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            if (propertyName.Contains("."))
            {
                propertyName = propertyName.Substring(propertyName.LastIndexOf(".") + 1);
            }

            PropertyInfo propertyInfo = properties.SingleOrDefault(x => x.Name.ToLower().Equals(propertyName.ToLower()));

            if (propertyInfo == null)
            {
                return null;
            }

            return propertyInfo.GetValue(actualObject);
        }

        private bool IsPropertyTranslatable(string propertyName)
        {
            bool isPropertyTranslatable = false;

            bool translateAllProperties = ((this.PropertiesToTranslate == null || this.PropertiesToTranslate.Count == 0)
                                            && (this.PropertiesToIgnore == null || this.PropertiesToIgnore.Count == 0)) ? true : false;

            if (translateAllProperties)
            {
                isPropertyTranslatable = true;
            }
            else
            {
                bool existsInPropertiesToTranslate = this.IfExistsInPropertiesToTranslate(propertyName);
                bool existsInPropertiesToIgnore = this.IfExistsInPropertiesToIgnore(propertyName);

                if (existsInPropertiesToTranslate && !existsInPropertiesToIgnore)
                {
                    // Property name will only be translated if it exists in PropertyNamesToTranslate and doesn't exist in PropertyNameToIgnore. Otherwise, it won't be translated.
                    isPropertyTranslatable = true;
                }
            }

            return isPropertyTranslatable;
        }

        private bool IfExistsInPropertiesToIgnore(string propertyName)
        {
            if (this.PropertiesToIgnore.Count == 0)
            {
                return false;
            }

            string propertyNameToMatch = this.GetPropertyNameToMatch(propertyName);

            return this.PropertiesToIgnore.Any(x => x.Trim().ToLower().Equals(propertyNameToMatch.Trim().ToLower()));
        }

        private bool IfExistsInPropertiesToTranslate(string propertyName)
        {
            if (this.PropertiesToTranslate.Count == 0)
            {
                return true;
            }

            string propertyNameToMatch = this.GetPropertyNameToMatch(propertyName);

            return this.PropertiesToTranslate.Any(x => x.Trim().ToLower().Equals(propertyNameToMatch.Trim().ToLower()));
        }

        private string GetPropertyNameToMatch(string propertyName)
        {
            if (propertyName.Contains("[") && propertyName.Contains("]"))
            {
                // The field is named PropertyNameWithoutIndex, even though its default value is the property name containing indices, because the goal is to remove all indices from it after the loop is processed.
                string propertyNameWithoutIndex = propertyName;
                string indexWithBrackets;

                do
                {
                    indexWithBrackets = this.GetIndexWithBrackets(propertyNameWithoutIndex);

                    propertyNameWithoutIndex = propertyNameWithoutIndex.Replace(indexWithBrackets, string.Empty);
                }
                while (propertyNameWithoutIndex.Contains("[") && propertyNameWithoutIndex.Contains("]"));

                return propertyNameWithoutIndex;
            }

            return propertyName;
        }

        private string GetIndexWithBrackets(string propertyName)
        {
            string indexWithBrackets = string.Empty;

            try
            {
                int indexOfStartBrace = propertyName.IndexOf("[");
                int indexOfEndBrace = propertyName.IndexOf("]");

                indexWithBrackets = propertyName.Substring(indexOfStartBrace, indexOfEndBrace - indexOfStartBrace + 1);
            }
            catch
            {
            }

            return indexWithBrackets;
        }

        private string GetFullyQualifiedNameOfProperty(PropertyInfo property, string objectName = null)
        {
            StringBuilder fullyQualifiedPropertyName = new StringBuilder();

            if (!string.IsNullOrEmpty(objectName))
            {
                fullyQualifiedPropertyName.Append(string.Format("{0}.", objectName));
            }

            fullyQualifiedPropertyName.Append(string.Format("{0}", property.Name));

            return fullyQualifiedPropertyName.ToString();
        }

        private void AddTypeIfNotExists(ref List<Type> parentTypes, Type objectType)
        {
            if (parentTypes == null)
            {
                parentTypes = new List<Type>();
            }

            if (!parentTypes.Contains(objectType))
            {
                parentTypes.Add(objectType);
            }
        }

        private int GetCollectionSize(dynamic collection)
        {
            Type collectionType = collection.GetType();

            if (collectionType.GetProperty("Count") != null)
            {
                return collection.Count;
            }
            else if (collectionType.GetProperty("Length") != null)
            {
                return collection.Length;
            }
            else
            {
                return 0;
            }
        }

        private IEnumerable<PropertyInfo> YieldPropertyInfo(IEnumerable<PropertyInfo> properties)
        {
            foreach (PropertyInfo property in properties)
            {
                yield return property;
            }
        }

        private List<PropertyInfo> GetShortListedProperties(Type type)
        {
            // Get only those properties that are non-primitive and not value types and are either derived from base classes (System.String and custom classes) or are collections/generics.
            return type.GetProperties().Where(x => !x.PropertyType.IsPrimitive
                                                    && !x.PropertyType.IsValueType
                                                    && (x.PropertyType.IsClass || x.PropertyType.IsGenericType)).ToList();
        }

        private bool IsTypeNonTranslatable(Type type)
        {
            return (type.IsPrimitive || type.IsValueType) ? true : false;
        }

        private bool IsNull(T entityToTranslate)
        {
            object entityToTranslateAsObject = entityToTranslate as object;

            if (entityToTranslateAsObject == null)
            {
                return true;
            }

            return false;
        }
    }
}