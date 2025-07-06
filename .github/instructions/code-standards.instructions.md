---
applyTo: '-'
---
# Coding Standards for Flare - Prototype

This document outlines the coding standards to be followed for the Flare - Prototype project. Adhering to these standards will help ensure the codebase is clean, readable, maintainable, and leverages Unity best practices.

## Documentation
- **XML Documentation Comments**
    - Public API: All public classes, methods, properties, and events must have XML documentation comments (`/// <summary>...</summary>`).
    - Clarity: Summaries should clearly and concisely explain the purpose and functionality.
    - Parameters & Returns: For methods, include `<param name="paramName">...</param>` for each parameter and `<returns>...</returns>` if the method returns a value.
    - Remarks: Use `<remarks>...</remarks>` for additional important information or usage notes.
- **Unity-Specific Documentation**
    - Serialized Fields: All `[SerializeField]` fields must have a `[Tooltip("...")]` attribute to explain their purpose in the Unity Inspector.
        ```csharp
        [Tooltip("The speed at which the player moves.")]
        [SerializeField] private float movementSpeed = 5f;
        ```

## Code Formatting & Style
- **Readability**
    - Indentation & Spacing: Use consistent indentation (4 spaces) and appropriate spacing (e.g., around operators, after commas).
    - Line Length: Keep lines of code to a reasonable length to avoid horizontal scrolling. Break long lines logically.
    - One Declaration Per Line: Each variable declaration should be on its own line.
        ```csharp
        // Preferred
        private int score;
        private string playerName;

        // Avoid
        // private int score; private string playerName;
        ```
- **Naming Conventions**
    - Clarity & Descriptiveness: Names (variables, methods, classes, etc.) must be clear, descriptive, and follow standard C# naming conventions:
        - `PascalCase` for classes, methods, properties, events, enums, and enum members.
        - `camelCase` for local variables and method parameters.
        - `_camelCase` for private instance fields (as a common convention, though `camelCase` without the underscore is also acceptable if consistent).
    - Avoid Ambiguous Abbreviations: Prefer full names over abbreviations unless the abbreviation is widely understood (e.g., `UI`, `ID`, `HTTP`).

## Unity Best Practices
- **MonoBehaviour Lifecycle**
    - Correct Usage: Understand and use Unity's MonoBehaviour lifecycle methods (`Awake`, `Start`, `Update`, `FixedUpdate`, `LateUpdate`, `OnEnable`, `OnDisable`, `OnDestroy`) appropriately.
    - Initialization:
        - `Awake()`: For self-initialization, `GetComponent<T>()` calls, and setting up internal references.
        - `Start()`: For initialization dependent on other scripts' `Awake()` or setup before the first frame.
    - Cleanup:
        - `OnDestroy()`: Clean up resources and unsubscribe from events to prevent memory leaks or errors.
- **Event Handling**
    - Subscription & Unsubscription: Always unsubscribe from events when the listener is no longer needed or is being destroyed (typically in `OnDisable` or `OnDestroy`).
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
- **Null Checks**
    - Defensive Programming: Perform null checks for critical references, especially those from `GetComponent<T>()`, `FindObjectOfType<T>()`, or public fields.
    - Error Logging: If a required reference is missing, log a clear error message using `Debug.LogError()`.
- **Performance Considerations (Prototyping Phase)**
    - Cache Components: Avoid frequent `GetComponent<T>()` calls in `Update()` or other frequently called methods. Cache references in `Awake()` or `Start()`.
    - Avoid Allocations in Update: Minimize creating new objects (e.g., `new List<T>()`, string concatenations) in `Update()` or similar methods to reduce garbage collection overhead.

## Code Cleanliness & Maintainability
- **Single Responsibility Principle (SRP)**
    - Strive for classes and methods to have one primary responsibility.
- **Don't Repeat Yourself (DRY)**
    - Avoid duplicating code. Extract repeated logic into reusable methods or classes.
- **Magic Numbers & Strings**
    - Avoid using raw numbers or strings directly in code if their meaning isn't immediately obvious.
    - Use `const`, `static readonly` fields, or enums to give them meaningful names.
        ```csharp
        // Avoid
        // if (playerState == 0) { /* ... */ }

        // Good
        // public const int PlayerStateIdle = 0;
        // if (playerState == PlayerStateIdle) { /* ... */ }

        // Even better (using an enum)
        // public enum PlayerStateType { Idle, Moving, Attacking }
        // if (currentState == PlayerStateType.Idle) { /* ... */ }
        ```
- **Comments for Complex Logic**
    - While self-documenting code is preferred, add comments to explain the *why* behind complex, non-obvious, or critical sections of logic. Comments should clarify intent, not just restate what the code does.
