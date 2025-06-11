# Enemy Spawning & Basic Combat Implementation Plan

## Overview
This document outlines the implementation of enemy spawning and basic combat mechanics for the Flare prototype, focusing on modular, event-driven architecture that will support the future card system.

## Core Components

### 1. Health System
**Purpose**: Manages entity health with configurable values, damage processing, and UI-friendly event system.

**Components**:
- `Health.cs` - Core health management component with event broadcasting
- `DamageNumber.cs` - Visual feedback for damage dealt
- `DamageNumberSpawner.cs` - Manages damage number instances
- `HealthDisplay.cs` - UI component for heart-based health visualization

**Key Features**:
- **Float-based Health**: Supports fractional values for half-heart display (0.0-5.0)
- **Event-Driven UI Updates**: Health change events for reactive UI systems
- **Damage Processing**: Immunity frames and damage validation
- **Visual Damage Feedback**: Floating damage numbers
- **Death Event Broadcasting**: Decoupled death notifications

**UI Integration**:
- Observable health changes via UnityEvent/C# events
- Health-to-hearts conversion utilities (2.5 health = 2.5 hearts)
- Support for heart UI animations (damage taken, healing, etc.)
- Configurable max health affects max heart display

### 2. Enemy System
**Purpose**: AI-driven enemies with state-based behavior and combat integration using Strategy pattern for maximum extensibility.

**Core Components**:
- `Enemy.cs` - Core enemy logic and state management
- `EnemyAI.cs` - State machine coordinator
- `EnemyStats.cs` - Configurable enemy statistics
- `EnemyStateContext.cs` - Shared data and utilities for state strategies

**Strategy Pattern Architecture**:
The AI system uses a multi-layered Strategy pattern with stateless, reusable sub-behaviors:

**Core Strategy Interfaces**:
- `ITargetingStrategy.cs` - Determines movement target/direction
- `IMovementStrategy.cs` - Handles how enemy moves to target
- `IAttackStrategy.cs` - Manages attack execution
- `IStateStrategy.cs` - Coordinates sub-strategies and state transitions

**Targeting Strategies** (Where to go):
- `WanderTargetingStrategy.cs` - Random wandering behavior
- `IdleTargetingStrategy.cs` - Stay in place
- `PursuePlayerDirectlyStrategy.cs` - Target player's current position
- `PursueAheadOfPlayerStrategy.cs` - Predict and intercept player
- `PursueWithAvoidanceStrategy.cs` - Target player while avoiding obstacles
- `PatrolTargetingStrategy.cs` - Follow predefined patrol points

**Movement Strategies** (How to get there):
- `WalkMovementStrategy.cs` - Constant speed movement
- `HopMovementStrategy.cs` - Interval-based hopping movement
- `DashMovementStrategy.cs` - Quick burst movement with cooldown
- `SmoothMovementStrategy.cs` - Accelerated movement with easing

**Attack Strategies** (How to deal damage):
- `MeleeSwingStrategy.cs` - Close-range attack with hitbox
- `ProjectileStrategy.cs` - Ranged projectile attacks
- `ChargeAttackStrategy.cs` - Dash-based damage dealing
- `AoeAttackStrategy.cs` - Area-of-effect damage

**AI State Management**:
- `EnemyAIState` enum: `PASSIVE`, `PURSUING`, `ATTACKING`, `STUNNED`
- Each enemy has assigned strategy combinations for each state
- Strategies can request state transitions by returning desired state
- Flyweight pattern: All enemies share stateless strategy instances
- State coordinator validates and executes requested transitions

**Strategy Composition Examples**:
- **Basic Grunt**: 
  - Passive: `WanderTargetingStrategy` + `WalkMovementStrategy`
  - Pursuing: `PursuePlayerDirectlyStrategy` + `WalkMovementStrategy`
  - Attacking: `MeleeSwingStrategy`
  
- **Hopping Enemy**:
  - Passive: `IdleTargetingStrategy` + `HopMovementStrategy`
  - Pursuing: `PursuePlayerDirectlyStrategy` + `HopMovementStrategy`
  - Attacking: `MeleeSwingStrategy`
  
- **Smart Archer**:
  - Passive: `PatrolTargetingStrategy` + `WalkMovementStrategy`
  - Pursuing: `PursueAheadOfPlayerStrategy` + `SmoothMovementStrategy`
  - Attacking: `ProjectileStrategy`

**Extensibility Benefits**:
- **Mix & Match**: Combine different strategies for unique enemy types
- **Reusability**: Same strategy can be used across multiple enemy types
- **Easy Testing**: Individual strategies can be unit tested in isolation
- **Runtime Flexibility**: Strategies can be changed based on difficulty, player actions, or special conditions
- **Designer Friendly**: Non-programmers can create new enemy types by combining existing strategies

**Configuration System**:
- Inspector-friendly strategy assignment on enemy prefabs
- Custom property drawers for easy strategy selection
- ScriptableObject-based strategy parameters for complex configurations
- Runtime strategy composition for dynamic difficulty scaling

**Performance Optimizations**:
- Stateless strategies shared between all enemy instances (Flyweight pattern)
- Single strategy instances managed by a `StrategyManager` singleton
- Memory-efficient operation with no per-enemy strategy allocation
- Easy pooling and caching of strategy results

**Key Features**:
- Configurable health, damage, speed, and detection ranges
- Line-of-sight player detection with customizable ranges
- Collision-based damage dealing
- Death particle effects
- Event-driven death notifications
- Strategy hot-swapping for dynamic difficulty

### 3. Player Combat System
**Purpose**: Modular, extensible combat system supporting multiple character types and attack styles.

**Core Components**:
- `PlayerCombat.cs` - Combat input handling and attack coordination
- `IPlayerAttack.cs` - Interface for all player attack types
- `AttackContext.cs` - Shared data for attack execution (input, timing, player stats)

**Attack Strategy Architecture**:
Using Strategy pattern for maximum extensibility across different character types:

**Attack Interfaces**:
- `IPlayerAttack.cs` - Base interface for all player attacks
- `IMeleeAttack.cs` - Extended interface for melee-specific functionality
- `IRangedAttack.cs` - Extended interface for ranged-specific functionality

**Melee Attack Implementations** (Current Scope):
- `SwordSwingAttack.cs` - Basic sword swing with directional hitbox
- `DaggerStabAttack.cs` - Fast, short-range stab attack
- `AxeSwingAttack.cs` - Slow, high-damage cleave attack

**Ranged Attack Implementations** (Future Scope):
- `BowAttack.cs` - Arrow projectiles with arc trajectory
- `CrossbowAttack.cs` - Fast, straight-line bolts
- `MagicMissileAttack.cs` - Homing magical projectiles

**Attack Execution Flow**:
1. `PlayerCombat` receives input and creates `AttackContext`
2. Current `IPlayerAttack` strategy executes with context
3. Attack handles damage dealing, visual effects, and cooldowns
4. Modular design allows easy swapping between character types

**Character Configuration**:
- Each character prefab has assigned attack strategy in inspector
- ScriptableObject-based attack configurations for designers
- Runtime attack swapping for character selection or card effects

**Key Features**:
- **Strategy-Based Design**: Easy character type creation and attack variations
- **Input Abstraction**: Same input system works for all attack types
- **Visual Consistency**: Shared attack indicator and feedback systems
- **Damage Integration**: Works seamlessly with existing Health system
- **Future Card Integration**: Attack strategies can be modified by card effects

### 4. Room Integration
**Purpose**: Connect enemy spawning with existing room clearing mechanics.

**Components**:
- Enhanced `NormalRoom.cs` - Enemy tracking and room state management
- `EnemySpawnManager.cs` - Coordinate spawning with existing `RoomContentPopulator`

**Key Features**:
- Event-driven enemy death tracking
- Automatic door unlocking when room cleared
- Integration with existing spawn point system
- Room state persistence

## Architecture Decisions

### Enemy AI Strategy Pattern Design
The multi-layered Strategy pattern with stateless sub-behaviors offers several advantages:

**Advantages**:
- **Maximum Reusability**: Mix and match targeting, movement, and attack behaviors independently
- **Memory Efficiency**: Stateless strategies shared via Flyweight pattern
- **Composition Flexibility**: Create unique enemy types by combining simple behaviors
- **Designer Friendly**: Inspector-based configuration without coding
- **Performance**: No per-enemy strategy allocation, shared instances
- **Testability**: Pure, stateless functions easy to unit test

**State Transition Design**:
- Strategies return `EnemyAIState?` to request state changes
- Central coordinator validates transition requests
- Strategies handle their own exit conditions (e.g., player in range, attack cooldown complete)
- Fallback to coordinator logic for complex multi-condition transitions

**Example State Flow**:
```
PASSIVE (WanderTargetingStrategy) → Player detected → Return PURSUING
PURSUING (PursuePlayerDirectlyStrategy) → In attack range → Return ATTACKING  
ATTACKING (MeleeSwingStrategy) → Attack complete → Return PURSUING
PURSUING (PursuePlayerDirectlyStrategy) → Player out of range → Return PASSIVE
```

**Alternative Patterns Considered**:
- **State Pattern**: Traditional state machine, but less flexible for mixing behaviors
- **Component System**: ECS-style approach, but adds complexity for this scope
- **Behavior Trees**: More complex but powerful, overkill for current needs
- **Command Pattern**: Good for actions, but Strategy is better for continuous behaviors

**Implementation Details**:
- Each strategy receives an `EnemyStateContext` containing shared data (transform, stats, player reference, etc.)
- Strategies implement `ExecuteState(EnemyStateContext context, float deltaTime)` method
- State transitions handled by central `EnemyAI` coordinator based on conditions
- ScriptableObject-based configuration for designer-friendly enemy creation

### Event System
- **Enemy Death Events**: `System.Action<Enemy>` delegates for loose coupling
- **Room Clear Events**: Broadcast when all enemies defeated
- **Damage Events**: Support future card effects and stat tracking

### Modular Combat
- **Interface-based Design**: `IDamageable`, `IAttacker` interfaces
- **Component Composition**: Separate concerns (health, damage, AI)
- **Easy Replacement**: Default attack system can be disabled/removed

### Visual Feedback
- **Particle Systems**: Modular death effects with expandable architecture
- **Damage Numbers**: Floating text with configurable styling
- **Attack Visualization**: Clear attack ranges and timing

## Technical Debt
- Refactor `PlayerController2D.OnMove` to use `InputActionReference` instead of relying on `PlayerInput` component's UnityEvent/SendMessage behavior for consistency and better C# control.

## Implementation Steps

1. **Core Health System**
   - Implement `Health.cs` with float-based health and event broadcasting
   - Create damage number visual feedback system
   - Add immunity frames and death handling
   - Implement health-to-hearts conversion utilities for UI

2. **Player Combat Foundation**
   - Create `IPlayerAttack` interface and attack strategy architecture
   - Implement `SwordSwingAttack.cs` as the primary melee attack
   - Create `PlayerCombat.cs` for input handling and attack coordination
   - Integrate with existing PlayerController2D

3. **Enemy AI and Behavior**
   - Implement strategy interfaces (`ITargetingStrategy`, `IMovementStrategy`, `IAttackStrategy`)
   - Create basic concrete strategies (wander, pursue, walk, melee)
   - Implement `EnemyAI.cs` state coordinator with strategy composition
   - Add player detection and damage dealing

4. **Room Integration and Polish**
   - Enhance `NormalRoom` with enemy tracking via death events
   - Implement door locking/unlocking mechanics
   - Add particle effects for enemy death
   - Create basic enemy prefabs with strategy configurations

5. **Visual Polish**
   - Add particle effects for enemy death
   - Implement damage number spawning
   - Create attack effect visualization

## Future Extensibility

### Card System Integration
- Combat interfaces support card-based damage dealing
- Event system allows cards to react to enemy deaths
- Modular attack system easily replaceable

### Advanced Features
- **Screen Shake**: Event-driven architecture supports easy addition
- **Sound Effects**: Audio components can subscribe to combat events  
- **Stat Tracking**: Death events support statistical collection
- **Visual Effects**: Particle system designed for expansion

## Testing Strategy

### Unit Testing
- Health component damage processing
- Enemy AI state transitions
- Attack collision detection

### Integration Testing
- Enemy spawning in rooms
- Room clearing mechanics
- Player-enemy combat interactions

### Playtest Validation
- Enemy AI feels responsive and challenging
- Combat feedback is clear and satisfying
- Room progression works smoothly
- Performance remains stable with multiple enemies

## Performance Considerations

### Object Pooling
- Enemy instances can be pooled for performance
- Damage numbers use pooling system
- Particle effects managed efficiently

### Update Optimization
- Enemy AI uses distance-based update frequency
- Line-of-sight checks optimized with layer masks
- Collision detection uses appropriate physics layers

This implementation plan ensures a solid foundation for combat mechanics while maintaining the flexibility needed for the card-based combat system that will follow.
