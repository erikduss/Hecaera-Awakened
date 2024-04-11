using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Erikduss
{
    public class GroundIndicator : NetworkBehaviour
    {
        public NetworkVariable<bool> objectEnabled = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> networkSize = new NetworkVariable<float>(2f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


        [SerializeField] GameObject childObjectToScale;

        [SerializeField] public float indicatorSize = 1f;

        protected virtual void Update()
        {
            //Projectiles are being handles by the server only!, assign its network position to the position of our transform.
            if (NetworkManager.Singleton.IsServer)
            {
                networkPosition.Value = transform.position;
                networkRotation.Value = transform.rotation;
            }
            //if the character is being controlled from elsewhere, then assign its position here locally.
            else
            {
                transform.position = networkPosition.Value;
                transform.rotation = networkRotation.Value;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!NetworkManager.Singleton.IsServer)
            {
                if (transform.position != networkPosition.Value)
                {
                    transform.position = networkPosition.Value;
                }

                if (transform.rotation != networkRotation.Value)
                {
                    transform.rotation = networkRotation.Value;
                }

                //if we're active but the network variable says we shouldnt be active. Disable.
                if (gameObject.activeSelf && !objectEnabled.Value)
                {
                    gameObject.SetActive(false);
                }

                objectEnabled.OnValueChanged += OnObjectEnabledChange;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                objectEnabled.OnValueChanged -= OnObjectEnabledChange;
                return;
            }

            base.OnNetworkDespawn();
        }

        public void OnObjectEnabledChange(bool oldID, bool newID)
        {
            gameObject.SetActive(newID);

            if (!NetworkManager.Singleton.IsServer)
                return;
        }

        public virtual void SetIndicatorSize(float size)
        {
            childObjectToScale.transform.localScale = new Vector3(size, 5, size);
        }
    }
}
