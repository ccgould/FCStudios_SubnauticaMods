using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.FCSPDA.Mono.ScreenItems
{
    internal class EncyclopediaTabController : MonoBehaviour, uGUI_IListEntryManager
    {
        private CraftNode tree = new CraftNode("Root")
        {
            string0 = string.Empty,
            string1 = string.Empty,
            monoBehaviour0 = null,
            bool0 = false,
            int0 = 0
        };

        private uGUI_ListEntry selectedEntry
        {
            get
            {
                if (UISelection.selected != null && UISelection.selected.IsValid())
                {
                    return UISelection.selected as uGUI_ListEntry;
                }
                return null;
            }
        }
        private uGUI_ListEntry activeEntry;
        private Text title;
        private Text message;
        private RawImage image;
        public LayoutElement imageLayout;
        [AssertNotNull]
        public Texture2D defaultTexture;
        public Sprite iconExpand;
        public float indentStep = 10f;
        private List<uGUI_ListEntry> pool = new List<uGUI_ListEntry>();
        public UISpriteData pathNodeSprites;
        public UISpriteData entryNodeSprites;
        public RectTransform listCanvas;
        public GameObject prefabEntry;
        private object selectedItem;
        public ScrollRect listScrollRect;
        public Sprite iconCollapse;

        public bool OnButtonDown(string key, GameInput.Button button)
        {
            CraftNode craftNode = this.tree.FindNodeById(key, false) as CraftNode;
            if (craftNode == null)
            {
                return false;
            }
            uGUI_EncyclopediaTab.GetNodeListEntry(craftNode);
            TreeAction action = craftNode.action;
            if (button != GameInput.Button.LeftHand)
            {
                switch (button)
                {
                    case GameInput.Button.UISubmit:
                        goto IL_4C;
                    case GameInput.Button.UILeft:
                        if (action == TreeAction.Expand)
                        {
                            if (uGUI_EncyclopediaTab.GetNodeExpanded(craftNode))
                            {
                                this.ToggleExpand(craftNode);
                            }
                            else
                            {
                                CraftNode craftNode2 = craftNode.parent as CraftNode;
                                if (craftNode2 != null && craftNode2.action == TreeAction.Expand)
                                {
                                    this.SelectItem(uGUI_EncyclopediaTab.GetNodeListEntry(craftNode2));
                                }
                            }
                        }
                        else if (action == TreeAction.Craft)
                        {
                            CraftNode craftNode3 = craftNode.parent as CraftNode;
                            if (craftNode3 != null && craftNode3.action == TreeAction.Expand)
                            {
                                this.SelectItem(uGUI_EncyclopediaTab.GetNodeListEntry(craftNode3));
                            }
                        }
                        return true;
                    case GameInput.Button.UIRight:
                        if (action == TreeAction.Expand)
                        {
                            if (uGUI_EncyclopediaTab.GetNodeExpanded(craftNode))
                            {
                                using (IEnumerator<CraftNode> enumerator = craftNode.GetEnumerator())
                                {
                                    if (enumerator.MoveNext())
                                    {
                                        CraftNode node = enumerator.Current;
                                        this.SelectItem(uGUI_EncyclopediaTab.GetNodeListEntry(node));
                                    }
                                    return true;
                                }
                            }
                            this.ToggleExpand(craftNode);
                        }
                        else if (action == TreeAction.Craft)
                        {
                            this.Activate(craftNode);
                        }
                        return true;
                }
                return false;
            }
            IL_4C:
            if (action == TreeAction.Expand)
            {
                this.ToggleExpand(craftNode);
            }
            else if (action == TreeAction.Craft)
            {
                this.Activate(craftNode);
            }
            return true;
        }

        public void SelectItem(object item)
        {
            this.selectedItem = item;
            this.DeselectItem();
            uGUI_ListEntry uGUI_ListEntry = item as uGUI_ListEntry;
            if (uGUI_ListEntry != null)
            {
                UISelection.selected = uGUI_ListEntry;
                uGUI_ListEntry.OnPointerEnter(null);
                this.listScrollRect.ScrollTo(uGUI_ListEntry.rectTransform, true, false, new Vector4(10f, 10f, 10f, 10f));
                return;
            }
            if (this.selectedItem is Selectable)
            {
                (this.selectedItem as Selectable).Select();
                UISelection.selected = null;
            }
        }

        public void DeselectItem()
        {
            if (this.selectedEntry == null)
            {
                return;
            }
            this.selectedEntry.OnPointerExit(null);
            UISelection.selected = null;
        }

        private void ToggleExpand(CraftNode node)
        {
            if (uGUI_EncyclopediaTab.GetNodeExpanded(node))
            {
                this.Collapse(node);
                return;
            }
            this.Expand(node);
            this.UpdatePositions();
        }

        private void UpdatePositions()
        {
            using (IEnumerator<CraftNode> enumerator = this.tree.Traverse(false))
            {
                int num = 0;
                while (enumerator.MoveNext())
                {
                    CraftNode node = enumerator.Current;
                    uGUI_ListEntry nodeListEntry = uGUI_EncyclopediaTab.GetNodeListEntry(node);
                    if (nodeListEntry != null)
                    {
                        nodeListEntry.rectTransform.SetSiblingIndex(num);
                    }
                    num++;
                }
            }
        }

        private void Expand(CraftNode node)
        {
            uGUI_EncyclopediaTab.SetNodeExpanded(node, true);
            uGUI_ListEntry nodeListEntry = uGUI_EncyclopediaTab.GetNodeListEntry(node);
            if (nodeListEntry != null)
            {
                nodeListEntry.SetNotificationAlpha(0f);
                nodeListEntry.SetIcon(this.iconCollapse);
            }
            CraftNode node2 = PDAEncyclopedia.GetNode(node.id);
            if (node2 == null)
            {
                return;
            }
            foreach (CraftNode craftNode in node2)
            {
                string id = craftNode.id;
                CraftNode craftNode2 = node[id] as CraftNode;
                if (craftNode2 == null)
                {
                    craftNode2 = this.CreateNode(craftNode, node);
                }
                else
                {
                    uGUI_EncyclopediaTab.GetNodeListEntry(craftNode2).gameObject.SetActive(true);
                    if (uGUI_EncyclopediaTab.GetNodeExpanded(craftNode2))
                    {
                        this.Expand(craftNode2);
                    }
                }
            }
            node.Sort(uGUI_EncyclopediaTab.entryComparer);
        }

        private void Activate(CraftNode node)
        {
            if (node == null || node.action != TreeAction.Craft)
            {
                return;
            }
            uGUI_ListEntry nodeListEntry = uGUI_EncyclopediaTab.GetNodeListEntry(node);
            if (activeEntry != nodeListEntry)
            {
                if (activeEntry != null)
                {
                    activeEntry.SetSelected(false);
                }
                activeEntry = nodeListEntry;
                if (activeEntry != null)
                {
                    DisplayEntry(node.id);
                    activeEntry.SetSelected(true);
                }
            }
        }

        private void Collapse(CraftNode node)
        {
            uGUI_EncyclopediaTab.SetNodeExpanded(node, false);
            uGUI_ListEntry nodeListEntry = uGUI_EncyclopediaTab.GetNodeListEntry(node);
            if (nodeListEntry != null)
            {
                uGUI_EncyclopediaTab.UpdateNotificationsCount(nodeListEntry, uGUI_EncyclopediaTab.GetNodeNotificationsCount(node));
                nodeListEntry.SetIcon(this.iconExpand);
            }
            using (IEnumerator<CraftNode> enumerator = node.Traverse(false))
            {
                while (enumerator.MoveNext())
                {
                    CraftNode node2 = enumerator.Current;
                    uGUI_ListEntry nodeListEntry2 = uGUI_EncyclopediaTab.GetNodeListEntry(node2);
                    if (nodeListEntry2 != null)
                    {
                        nodeListEntry2.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void DisplayEntry(string key)
        {
            PDAEncyclopedia.EntryData entryData;
            if (key != null && PDAEncyclopedia.GetEntryData(key, out entryData))
            {
                if (entryData.timeCapsule)
                {
                    if (TimeCapsuleContentProvider.GetData(key, out var text, out var text2, out var text3))
                    {
                        SetTitle(text);
                        SetText(text2);
                        if (!string.IsNullOrEmpty(text3))
                        {
                            SetImage(defaultTexture);
                        }
                        else
                        {
                            SetImage(null);
                        }
                    }
                    else
                    {
                        SetTitle(entryData.key);
                        SetText(Language.main.Get("TimeCapsuleContentFetchError"));
                        SetImage(null);
                    }
                }
                else
                {
                    SetTitle(Language.main.Get("Ency_" + key));
                    SetText(Language.main.Get("EncyDesc_" + key));
                    SetImage(entryData.image);
                }
                SetAudio(entryData.audio);
                return;
            }
            SetTitle(string.Empty);
            SetText(string.Empty);
            SetImage(null);
            SetAudio(null);
        }

        private void SetTitle(string value)
        {
            title.text = value;
        }

        private void SetText(string value)
        {
            message.text = value;
        }

        private void SetImage(Texture2D texture)
        {
            image.texture = texture;
            if (texture != null)
            {
                float num = (float)texture.height / (float)texture.width;
                float num2 = image.rectTransform.rect.width * num;
                imageLayout.minHeight = num2;
                imageLayout.preferredHeight = num2;
                image.gameObject.SetActive(true);
                return;
            }
            image.gameObject.SetActive(false);
        }

        private void SetAudio(FMODAsset asset)
        {
            //this.SetAudioState(false, 0f, 0f);
            //if (asset != null && !string.IsNullOrEmpty(asset.id))
            //{
            //    this.audio = asset.id;
            //    this.audioContainer.SetActive(true);
            //    return;
            //}
            //this.audio = null;
            //this.audioContainer.SetActive(false);
        }

        private CraftNode CreateNode(CraftNode srcNode, CraftNode parentNode)
        {
            if (srcNode == null || parentNode == null)
            {
                return null;
            }
            string id = srcNode.id;
            if (parentNode[id] != null)
            {
                return null;
            }
            TreeAction action = srcNode.action;
            int depth = parentNode.depth;
            UISpriteData spriteData;
            Sprite icon;
            float indent;
            if (action == TreeAction.Expand)
            {
                spriteData = this.pathNodeSprites;
                icon = this.iconExpand;
                indent = (float)(depth + 1) * this.indentStep;
            }
            else
            {
                if (action != TreeAction.Craft)
                {
                    return null;
                }
                spriteData = this.entryNodeSprites;
                icon = null;
                indent = (float)depth * this.indentStep;
            }
            string @string = srcNode.string0;
            string string2 = srcNode.string1;
            uGUI_ListEntry entry = this.GetEntry();
            entry.Initialize(this, id, spriteData);
            entry.SetIcon(icon);
            entry.SetIndent(indent);
            entry.SetText(string2);
            int num = 0;
            CraftNode craftNode = new CraftNode(id, action, TechType.None)
            {
                string0 = @string,
                string1 = string2,
                monoBehaviour0 = entry,
                bool0 = false,
                int0 = num
            };
            parentNode.AddNode(new CraftNode[]
            {
                craftNode
            });
            if (action == TreeAction.Expand)
            {
                using (IEnumerator<CraftNode> enumerator = srcNode.Traverse(false))
                {
                    while (enumerator.MoveNext())
                    {
                        CraftNode craftNode2 = enumerator.Current;
                        if (craftNode2.action == TreeAction.Craft && NotificationManager.main.Contains(NotificationManager.Group.Encyclopedia, craftNode2.id))
                        {
                            num++;
                        }
                    }
                }
                uGUI_EncyclopediaTab.SetNodeNotificationsCount(craftNode, num);
                uGUI_EncyclopediaTab.UpdateNotificationsCount(entry, num);
            }
            else if (action == TreeAction.Craft)
            {
                NotificationManager.main.RegisterTarget(NotificationManager.Group.Encyclopedia, srcNode.id, entry);
            }
            return craftNode;
        }

        private uGUI_ListEntry GetEntry()
        {
            uGUI_ListEntry uGUI_ListEntry;
            if (this.pool.Count == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    uGUI_ListEntry = UnityEngine.Object.Instantiate<GameObject>(this.prefabEntry).GetComponent<uGUI_ListEntry>();
                    uGUI_ListEntry.rectTransform.SetParent(this.listCanvas, false);
                    uGUI_ListEntry.Uninitialize();
                    this.pool.Add(uGUI_ListEntry);
                }
            }
            int index = this.pool.Count - 1;
            uGUI_ListEntry = this.pool[index];
            this.pool.RemoveAt(index);
            return uGUI_ListEntry;
        }
    }
}
