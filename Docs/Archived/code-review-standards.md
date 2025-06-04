# Code Review Standards & Methodology

This document outlines the standards and methodology to be applied during code reviews for the Flare - Prototype project. The goal is to ensure code is clean, well-documented, maintainable, and adheres to Unity best practices.

## 1. Documentation

### 1.1. XML Documentation Comments
- **Public API:** All public classes, methods, properties, and events should have XML documentation comments (`/// <summary>...</summary>`).
- **Clarity:** Summaries should clearly and concisely explain the purpose and functionality of the code element.
- **Parameters & Returns:** For methods, include `<param name="paramName">...</param>` for each parameter and `<returns>...</returns>` if the method returns a value.
- **Remarks:** Use `<remarks>...</remarks>` for any additional important information, usage notes, or context that doesn't fit in the summary.

### 1.2. Unity-Specific Documentation
- **Serialized Fields:** All `[SerializeField]` fields should have a `[Tooltip("...")]` attribute to explain their purpose in the Unity Inspector. This greatly improves usability for designers and other developers.
    ```csharp
    [Tooltip("The speed at which the player moves.")]
    [SerializeField] private float movementSpeed = 5f;
    ```

## 2. Code Formatting & Style

### 2.1. Readability
- **Indentation & Spacing:** Use consistent indentation (typically 4 spaces) and appropriate spacing around operators and after commas to enhance readability.
- **Line Length:** Avoid excessively long lines of code. Break them down for clarity.
- **One Declaration Per Line:** Each variable declaration should be on its own line.
    ```csharp
    // Good
    private int score;
    private string playerName;

    // Avoid
    // private int score; private string playerName;
    ```

### 2.2. Naming Conventions
- **Clarity & descriptiveness:** Names (variables, methods, classes) should be clear, descriptive, and follow standard C# naming conventions (e.g., PascalCase for classes and methods, camelCase for local variables and private fields).
- **Avoid abbreviations:** Unless an abbreviation is extremely common and well-understood (e.g., UI, ID), prefer full names.

## 3. Unity Best Practices

### 3.1. MonoBehaviour Lifecycle
- **Correct Usage:** Understand and correctly use Unity's MonoBehaviour lifecycle methods (`Awake`, `Start`, `Update`, `FixedUpdate`, `LateUpdate`, `OnEnable`, `OnDisable`, `OnDestroy`, etc.).
- **Initialization:**
    - `Awake()`: Use for self-initialization of a script's components and variables. Ideal for `GetComponent<T>()` calls and setting up internal references.
    - `Start()`: Use for initialization that depends on other scripts having completed their `Awake()` phase, or for setup that needs to happen just before the first frame.
- **Cleanup:**
    - `OnDestroy()`: Use to clean up resources, unsubscribe from events, or perform any other necessary actions when a GameObject or Component is destroyed. This is crucial for preventing memory leaks or unintended behavior.

### 3.2. Event Handling
- **Subscription & Unsubscription:** Always unsubscribe from events when the listener is no longer needed or is being destroyed (typically in `OnDisable` or `OnDestroy`). This prevents null reference exceptions if the event publisher outlives the subscriber, or if the event is triggered on a disabled object.
    ```csharp
    void OnEnable()
    {
        SomeManager.OnSomethingHappened += HandleSomethingHappened;
    }

    void OnDisable()
    {
        SomeManager.OnSomethingHappened -= HandleSomethingHappened;
    }
    ```

### 3.3. Null Checks
- **Defensive Programming:** Perform null checks for critical references, especially those obtained via `GetComponent<T>()`, `FindObjectOfType<T>()`, or public fields that might not be assigned in the Inspector.
- **Clear Error Handling:** If a required component or reference is missing, log a clear error message using `Debug.LogError()` to aid in debugging.

### 3.4. Performance Considerations (Prototyping Phase)
- While deep optimization might be premature in early prototyping, be mindful of:
    - **Frequent `GetComponent<T>()` calls in `Update()`:** Cache component references in `Awake()` or `Start()`.
    - **Unnecessary allocations in `Update()`:** Avoid creating new objects (e.g., `new List<T>()`, string concatenations) in frequently called methods like `Update()`.

## 4. Code Cleanliness & Maintainability

### 4.1. Single Responsibility Principle (SRP)
- Classes and methods should ideally have one primary responsibility. This makes them easier to understand, test, and maintain.

### 4.2. Don't Repeat Yourself (DRY)
- Avoid duplicating code. If you find yourself writing the same logic in multiple places, consider extracting it into a reusable method or class.

### 4.3. Magic Numbers/Strings
- Avoid using raw numbers or strings directly in code if their meaning isn't immediately obvious.
- Use `const` or `static readonly` fields, or enums, to give them meaningful names.
    ```csharp
    // Avoid
    // if (playerState == 0) { /* ... */ }

    // Good
    // public const int PlayerStateIdle = 0;
    // if (playerState == PlayerStateIdle) { /* ... */ }
    // Or even better, use an enum.
    ```

### 4.4. Comments for Complex Logic
- While self-documenting code (clear naming, small methods) is preferred, add comments to explain *why* complex or non-obvious logic is implemented in a certain way. Comments should explain the intent, not just re-state what the code does.

## Review Process Steps

1.  **Understand Context:** Briefly understand the purpose of the script(s) being reviewed and their role in the overall feature.
2.  **Automated Checks:** (If applicable) Run any linters or static analysis tools.
3.  **Manual Review (using standards above):**
    *   Read through the code, focusing on documentation, clarity, and adherence to Unity best practices.
    *   Check for potential bugs, null reference issues, or performance concerns.
    *   Ensure event subscriptions are properly managed.
    *   Verify that Inspector-exposed fields are clear and well-documented with tooltips.
4.  **Provide Constructive Feedback:** Offer specific, actionable suggestions for improvement.

By following these standards, we can build a more robust, understandable, and maintainable codebase for "Flare - Prototype".
