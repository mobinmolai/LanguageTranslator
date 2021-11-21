namespace LanguageTranslator.Helper.Multilingual
{
   using Newtonsoft.Json;

    /// <summary>
    /// Helper class to make a deep copy.
    /// </summary>
    public static class DeepCopyHelper
    {
        private static readonly object ClonedObjectLock = new object();

        /// <summary>
        /// Clone the object passed.
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="objectToClone">Object to clone</param>
        /// <returns>Clone of the object passed.</returns>
        public static T Clone<T>(T objectToClone)
        {
            lock (ClonedObjectLock)
            {
                T clonedObject;

                try
                {
                    string objectToCloneString = JsonConvert.SerializeObject(objectToClone);

                    clonedObject = JsonConvert.DeserializeObject<T>(objectToCloneString);
                }
                catch
                {
                    clonedObject = objectToClone;
                }

                return clonedObject;
            }
        }
    }
}
