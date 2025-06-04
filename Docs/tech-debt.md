# Tech Debt

More things to do once we start making the actual game:
- Implement additive scene management (i.e. consolidating gameplay components into one scene and UI components into another to increase testability and decrease coupling)
  - For example, move all of the minimap logic into a new scene since in reality, all the minimap needs to know is 1) when to add a new room to the minimap, 2) where that room should go, 3) how big the new room is, and 4) what special icons to display for that room. We can decouple it from the StageGrid entirely and keep it separate.
