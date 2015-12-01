using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Stormancer;
using System.Threading.Tasks;

namespace Stormancer
{
    public class StormancerClientProvider : MonoBehaviour
    {
        public string AccountId = "";
        public string ApplicationName = "";

        void Awake()
        {
            ClientProvider.SetAccountId(AccountId);
            ClientProvider.SetApplicationName(ApplicationName);
        }
    }
}
