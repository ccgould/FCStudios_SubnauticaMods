using System.Collections.Generic;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Mods.OxygenTank.Mono
{
    internal class OxygenTankAttachPoint : MonoBehaviour, IPipeConnection
    {
        private readonly List<string>_children = new List<string>();
        private GameObject _attachPoint;
        public string parentPipeUID;
        public string rootPipeUID;
        public Vector3 parentPosition;
        public bool allowConnection = true;

        private void Start()
        {
            _attachPoint = gameObject.FindChild("Cube");
        }

        public void SetParent(IPipeConnection parent)
        {
            IPipeConnection parent2 = this.GetParent();
            this.parentPipeUID = null;
            this.rootPipeUID = null;
            if (parent2 != null && parent != parent2)
            {
                parent2.RemoveChild(this);
            }

            GameObject gameObject = parent?.GetGameObject();
            if (gameObject != null && gameObject.TryGetComponent(out OxygenPipe oxygenPipe))
            {
                Vector3 attachpoint = this.GetAttachPoint();
                Vector3 vector = Vector3.Normalize(oxygenPipe.parentPosition - attachpoint);
                float magnitude = (oxygenPipe.parentPosition - attachpoint).magnitude;
                oxygenPipe.transform.position = attachpoint;
                oxygenPipe.topSection.rotation = Quaternion.LookRotation(vector, Vector3.up);
                oxygenPipe.endCap.rotation = oxygenPipe.topSection.rotation;
                oxygenPipe.bottomSection.rotation = Quaternion.LookRotation(vector, Vector3.up);
                oxygenPipe.bottomSection.position = oxygenPipe.parentPosition;
                oxygenPipe.stretchedPart.position = oxygenPipe.topSection.position + vector;
                Vector3 localScale = oxygenPipe.stretchedPart.localScale;
                localScale.z = magnitude - 2f;
                oxygenPipe.stretchedPart.localScale = localScale;
                oxygenPipe.stretchedPart.rotation = oxygenPipe.topSection.rotation;


                this.parentPipeUID = gameObject.GetComponent<UniqueIdentifier>().Id;
                this.rootPipeUID = ((oxygenPipe.GetRoot() != null) ? oxygenPipe.GetRoot().GetGameObject().GetComponent<UniqueIdentifier>().Id : null);
                this.parentPosition = oxygenPipe.GetAttachPoint();
                oxygenPipe.AddChild(this);
            }
        }

        public IPipeConnection GetParent()
        {
            UniqueIdentifier uniqueIdentifier;
            if (!string.IsNullOrEmpty(this.parentPipeUID) && UniqueIdentifier.TryGetIdentifier(this.parentPipeUID, out uniqueIdentifier))
            {
                return uniqueIdentifier.GetComponent<IPipeConnection>();
            }
            return null;
        }

        public void SetRoot(IPipeConnection root)
        {
            if (root == null)
            {
                this.rootPipeUID = null;
            }
            else
            {
                this.rootPipeUID = root.GetGameObject().GetComponent<UniqueIdentifier>().Id;
            }
        }

        public IPipeConnection GetRoot()
        {
            return null;
        }

        public void AddChild(IPipeConnection child)
        {
            _children.Add(child.GetGameObject().GetComponent<UniqueIdentifier>().Id);
        }

        public void RemoveChild(IPipeConnection child)
        { 
            _children.Add(child.GetGameObject().GetComponent<UniqueIdentifier>().Id);
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public bool GetProvidesOxygen()
        {
            IPipeConnection parent = this.GetParent();
            if(parent != null && parent.GetProvidesOxygen())
            {
                return true;
            }
            return false;
        }

        public void Update()
        {
            IPipeConnection parent = null;
            if (allowConnection && this.parentPipeUID is null)
            {
                float num = 1000f;
                int num2 = UWE.Utils.OverlapSphereIntoSharedBuffer(base.transform.position, 1f, -1, QueryTriggerInteraction.UseGlobal);
                for (int i = 0; i < num2; i++)
                {
                    GameObject entityRoot = UWE.Utils.GetEntityRoot(UWE.Utils.sharedColliderBuffer[i].gameObject);
                    if (!(entityRoot == null))
                    {
                        IPipeConnection component = entityRoot.GetComponent<IPipeConnection>();
                        if (component != null && component.GetRoot() != null && this.IsInSight(component))
                        {
                            float magnitude = (entityRoot.transform.position - base.transform.position).magnitude;
                            if (magnitude < num)
                            {
                                parent = component;
                                num = magnitude;
                            }
                        }
                    }
                }
                if(parent != null)
                    this.SetParent(parent);

                return;
            }

            if (this.parentPipeUID != null && rootPipeUID is null)
            {
                GameObject gameObject = GetParent()?.GetGameObject();
                if (gameObject != null && gameObject.TryGetComponent(out OxygenPipe oxygenPipe))
                {
                    Vector3 attachpoint = this.GetAttachPoint();
                    Vector3 vector = Vector3.Normalize(oxygenPipe.parentPosition - attachpoint);
                    float magnitude = (oxygenPipe.parentPosition - attachpoint).magnitude;
                    oxygenPipe.transform.position = attachpoint;
                    oxygenPipe.topSection.rotation = Quaternion.LookRotation(vector, Vector3.up);
                    oxygenPipe.endCap.rotation = oxygenPipe.topSection.rotation;
                    oxygenPipe.bottomSection.rotation = Quaternion.LookRotation(vector, Vector3.up);
                    oxygenPipe.bottomSection.position = oxygenPipe.parentPosition;
                    oxygenPipe.stretchedPart.position = oxygenPipe.topSection.position + vector;
                    Vector3 localScale = oxygenPipe.stretchedPart.localScale;
                    localScale.z = magnitude - 2f;
                    oxygenPipe.stretchedPart.localScale = localScale;
                    oxygenPipe.stretchedPart.rotation = oxygenPipe.topSection.rotation;


                    this.parentPipeUID = gameObject.GetComponent<UniqueIdentifier>().Id;
                    this.rootPipeUID = ((oxygenPipe.GetRoot() != null) ? oxygenPipe.GetRoot().GetGameObject().GetComponent<UniqueIdentifier>().Id : null);
                    this.parentPosition = oxygenPipe.GetAttachPoint();
                }
            }
        }

        private bool IsInSight(IPipeConnection parent)
        {
            bool result = true;
            Vector3 value = parent.GetAttachPoint() - this.GetAttachPoint();
            int num = UWE.Utils.RaycastIntoSharedBuffer(new Ray(this.GetAttachPoint(), Vector3.Normalize(value)), value.magnitude, -5, QueryTriggerInteraction.UseGlobal);
            for (int i = 0; i < num; i++)
            {
                if (UWE.Utils.GetEntityRoot(UWE.Utils.sharedHitBuffer[i].collider.gameObject) != parent.GetGameObject())
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        public Vector3 GetAttachPoint()
        {
            return _attachPoint.transform.position;
        }

        public void UpdateOxygen()
        {
        }

        public bool HasAttachment()
        {
            return _children.Count > 0;
        }
    }
}