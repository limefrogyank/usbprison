using Lib.Net.Http.WebPush;
using SQLite;

namespace usbprison.aspnetcore.Model
{
    public class PushSubscription : Lib.Net.Http.WebPush.PushSubscription
    {
        #region Properties
        /// <summary>
        /// Gets or sets the subscription endpoint.
        /// </summary>
        [PrimaryKey] public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets client keys shared as part of subscription.
        /// </summary>
        [Ignore] public IDictionary<string, string> Keys { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Gets specific client key shared as part of subscription.
        /// </summary>
        /// <param name="keyName">The key name.</param>
        /// <returns>The key.</returns>
        public string GetKey(PushEncryptionKeyName keyName)
        {
            string key = null;

            if (Keys != null)
            {
                string keyNameStringified = StringifyKeyName(keyName);

                if (Keys.ContainsKey(keyNameStringified))
                {
                    key = Keys[keyNameStringified];
                }
                else
                {
                    key = Keys.SingleOrDefault(x => String.Equals(x.Key, keyNameStringified, StringComparison.OrdinalIgnoreCase)).Value;
                }
            }

            return key;
        }

        /// <summary>
        /// Sets specific client key shared as part of subscription.
        /// </summary>
        /// <param name="keyName">The key name.</param>
        /// <param name="key">The key.</param>
        public void SetKey(PushEncryptionKeyName keyName, string key)
        {
            if (Keys == null)
            {
                Keys = new Dictionary<string, string>();
            }

            Keys[StringifyKeyName(keyName)] = key;
        }

        private string StringifyKeyName(PushEncryptionKeyName keyName)
        {
            return keyName.ToString().ToLowerInvariant();
        }
        #endregion
    }
}
