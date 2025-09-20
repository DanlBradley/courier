using GameServices;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using WorldGeneration;

namespace UI.States
{
    public class RoutePlannerUIState : UIState
    {
        public override string StateName => "Route Planner";

        private GameObject regionBtnPf;
        private Transform routePlannerPanel;
        private TMP_Text regionDescriptionText;
        private float regionBtnSize;
        private float regionBtnOffset;

        private readonly List<GameObject> regionButtons = new();
        private Vector2Int? selectedRegion;
        private Vector2Int localGridCenterRegion;
        private int reinitializeThreshold = 0;
        
        private GameObject selectedButton;
        private GameObject currentRegionButton;
        
        private WorldManagerService wms;
        private LocalRegionData localData;
        
        public override void Initialize(UIService service)
        {
            base.Initialize(service);
            
            routePlannerPanel = service.GetRoutePlannerPanel();
            regionBtnPf = service.GetRegionBtnPf();
            wms = ServiceLocator.GetService<WorldManagerService>();
            if (!InitializeUIComponents()) return;
            CreateRegionButtons();
        }
        
        private bool InitializeUIComponents()
        {
            var worldMapPanel = routePlannerPanel.Find("WorldMap");
            if (worldMapPanel == null) { Debug.LogError("WorldMap Panel missing!"); return false; }
            
            var confirmBtn = routePlannerPanel.Find("GUIPanel/Confirm");
            if (confirmBtn == null) { Debug.LogError("Confirm Btn missing!"); return false; }
            
            var clearBtn = routePlannerPanel.Find("GUIPanel/ClearRoute");
            if (clearBtn == null) { Debug.LogError("ClearRoute Btn missing!"); return false; }

            var textBox = routePlannerPanel.Find("GUIPanel/RegionInfo");
            if (textBox == null) {Debug.LogError("Region textbox missing!"); return false; }
            
            regionDescriptionText = textBox.GetComponent<TMP_Text>();
            confirmBtn.GetComponent<Button>().onClick.AddListener(ConfirmRoute);
            clearBtn.GetComponent<Button>().onClick.AddListener(ClearRoute);
            return true;
        }
        
        private void CreateRegionButtons()
        {
            localData = wms.GetLocalRegionData();
            if (localData == null) { Debug.LogWarning("Failed to get local region data"); return; }
            
            var worldMapPanel = routePlannerPanel.Find("WorldMap");
            CalculateButtonDimensions(worldMapPanel);
            
            for (int localX = 0; localX < localData.size; localX++)
            {
                for (int localY = 0; localY < localData.size; localY++)
                {
                    var region = localData.GetRegion(localX, localY);
                    if (region == null) continue; // Skip empty spots near world edges
                    
                    GameObject btnObj = CreateRegionButton(localX, localY, worldMapPanel, region);
                    
                    if (localX == localData.playerLocalPosition.x && localY == localData.playerLocalPosition.y)
                    {
                        currentRegionButton = btnObj;
                    }
                    regionButtons.Add(btnObj);
                }
            }
        }
        
        private void CalculateButtonDimensions(Transform worldMapPanel)
        {
            RectTransform panelRect = worldMapPanel.GetComponent<RectTransform>();
            if (panelRect == null) { Debug.LogError("WorldMapPanel missing RectTransform!"); return; }
            
            float panelWidth = panelRect.rect.width;
            float panelHeight = panelRect.rect.height;
            float availableSpace = Mathf.Min(panelWidth, panelHeight);
            regionBtnSize = availableSpace / localData.size;
            // regionBtnSize = Mathf.Max(regionBtnSize, 8f);
        }
        
        private GameObject CreateRegionButton(int localX, int localY, Transform parent, Region region)
        {
            GameObject btnObj = Object.Instantiate(regionBtnPf, parent);
            RectTransform rectTransform = btnObj.GetComponent<RectTransform>();
            
            // Set size and position
            rectTransform.sizeDelta = new Vector2(regionBtnSize, regionBtnSize);
            float posX = localX * (regionBtnSize + regionBtnOffset);
            // Invert Y to match grid coordinate system (bottom-left origin)
            float posY = -(localData.size - 1 - localY) * (regionBtnSize + regionBtnOffset);
            rectTransform.anchoredPosition = new Vector2(posX, posY);
            
            // Setup button component
            Button button = btnObj.GetComponent<Button>();
            if (button != null)
            {
                SetupButtonAppearance(button, region);
                
                // Convert local coordinates to world coordinates for the click handler
                Vector2Int worldCoords = localData.LocalToWorldCoords(localX, localY);
                button.onClick.AddListener(() => OnRegionButtonClicked(worldCoords, btnObj, region.GetRegionInfo()));
            }
            
            btnObj.SetActive(false);
            return btnObj;
        }
        
        private void SetupButtonAppearance(Button button, Region region)
        {
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage == null) return;
            var regionSprite = region.GetRegionSprite();
            if (regionSprite != null) { buttonImage.sprite = regionSprite; }
        }
        
        public override void OnEnter()
        {
            if (ShouldReinitialize())
            {
                ClearExistingButtons();
                CreateRegionButtons();
            }
            routePlannerPanel.gameObject.SetActive(true);
            foreach (var button in regionButtons.Where(button => button != null))
            { button.SetActive(true); }
            
            HighlightCurrentRegion();
            
            regionDescriptionText.text = "";
            GameStateManager.Instance.ChangeState(GameState.UI);
        }

        public override void OnExit()
        {
            regionDescriptionText.text = "";
            foreach (var button in regionButtons.Where(button => button != null))
            { button.SetActive(false); }
            routePlannerPanel.gameObject.SetActive(false);
        }
        
        public override bool HandleInput(string inputAction)
        {
            switch (inputAction)
            {
                case "Cancel":
                    var uiStateManager = uiService.uiStateManager;
                    uiStateManager.TransitionToState<ExplorationUIState>();
                    return true;
                default: return false;
            }
        }
        
        private bool ShouldReinitialize()
        {
            var currentRegion = wms.GetCurrentRegion();
            if (currentRegion == null) return false;
    
            Vector2Int currentPos = new Vector2Int(currentRegion.gridX, currentRegion.gridY);
    
            if (regionButtons.Count == 0 || 
                Vector2Int.Distance(currentPos, localGridCenterRegion) > reinitializeThreshold)
            { localGridCenterRegion = currentPos; return true; }
            return false;
        }
        
        private void ClearExistingButtons()
        {
            foreach (var button in regionButtons.Where(button => button != null))
            { Object.Destroy(button); }
            regionButtons.Clear();
            selectedButton = null;
            currentRegionButton = null;
            selectedRegion = null;
        }
        
        private void HighlightCurrentRegion()
        {
            if (currentRegionButton == null) return;
            Transform currentBorder = currentRegionButton.transform.Find("CurrentBorder");
            if (currentBorder != null) { currentBorder.gameObject.SetActive(true); }
        }
        
        private void OnRegionButtonClicked(Vector2Int regionCoords, GameObject buttonObj, string regionInfo)
        {
            ClearSelection();
            selectedRegion = regionCoords;
            selectedButton = buttonObj;
            
            Transform selectedBorder = selectedButton.transform.Find("SelectedBorder");
            if (selectedBorder != null) { selectedBorder.gameObject.SetActive(true); }

            regionDescriptionText.text = regionInfo;
            
            Debug.Log($"Selected region: ({regionCoords.x}, {regionCoords.y}) [World Coordinates]");
        }
        
        private void ClearSelection()
        {
            if (selectedButton != null)
            {
                Transform selectedBorder = selectedButton.transform.Find("SelectedBorder");
                if (selectedBorder != null) { selectedBorder.gameObject.SetActive(false); }
            }
            
            selectedRegion = null;
            selectedButton = null;
        }
        
        private void ConfirmRoute()
        {
            if (!selectedRegion.HasValue) { Debug.LogWarning("No region selected for route planning"); return; }
            
            if (wms != null)
            {
                wms.GenerateRoute(selectedRegion.Value);
                Debug.Log($"Route generated to region: ({selectedRegion.Value.x}, {selectedRegion.Value.y})");
                ClearSelection();
            }
            else { Debug.LogError("WorldManagerService not found!"); }
        }
        
        private void ClearRoute()
        {
            if (wms != null)
            {
                wms.ClearRoute();
                ClearSelection();
            }
            else { Debug.LogError("WorldManagerService not found!"); }
        }
    }
}