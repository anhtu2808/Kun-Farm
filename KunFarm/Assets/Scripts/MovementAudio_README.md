# Movement Audio System

## Overview
The MovementAudio system provides footstep sound effects for character movement in the Kun-Farm game. It integrates seamlessly with the existing Movement.cs system to play footstep sounds when the player moves.

## Features
- ✅ Automatic footstep sounds when character moves
- ✅ Configurable volume and step frequency
- ✅ Support for multiple footstep sound clips for variety
- ✅ Pitch variation for more natural sound
- ✅ Automatic silence when character stops moving
- ✅ Performance optimized with timer-based audio playback
- ✅ Inspector-configurable settings
- ✅ Runtime audio control methods

## Setup Instructions

### 1. Add MovementAudio Component
1. Select your Player GameObject in the scene
2. Add the `MovementAudio` component in the Inspector
3. The component will automatically find the `Movement` component

### 2. Configure Audio Settings
In the MovementAudio component inspector:
- **Footstep Sounds**: Drag audio clips for footstep sounds (supports multiple clips for variety)
- **Volume**: Set the volume level (0-1)
- **Step Frequency**: Time between footstep sounds in seconds (0.1-2)
- **Pitch Variation**: Random pitch variation for natural sound (0-0.5)

### 3. Audio Files
Place footstep audio files in the `Assets/Audio/` directory and assign them to the Footstep Sounds array.

## Component Reference

### Public Fields
- `AudioClip[] footstepSounds` - Array of footstep audio clips
- `float volume` - Volume of footstep sounds (0-1)
- `float stepFrequency` - Time between footstep sounds (seconds)
- `float pitchVariation` - Random pitch variation range
- `Movement movementComponent` - Reference to Movement component (auto-found)

### Public Methods
- `SetVolume(float newVolume)` - Update volume at runtime
- `SetStepFrequency(float newFrequency)` - Update step frequency at runtime
- `SetPitchVariation(float newVariation)` - Update pitch variation at runtime
- `IsConfigured()` - Check if component is properly configured
- `PlayTestFootstep()` - Play a test footstep sound

## Integration with Movement.cs

The MovementAudio system uses the `Movement.IsMoving()` method to detect when the player is moving. This ensures:
- Footstep sounds only play when the character is actually moving
- Sounds automatically stop when the character stops
- Perfect synchronization with the movement system

## Performance Considerations

- Uses timer-based audio playback to avoid excessive audio calls
- AudioSource component is automatically managed
- Supports audio pooling through Unity's PlayOneShot method
- Minimal overhead when character is not moving

## Testing

Use the `MovementAudioTest` component for testing:
- F1: Play test footstep
- F2: Test volume control
- F3: Test frequency control

The test component provides debug information and validation methods.

## Compatibility

- ✅ Compatible with existing Movement.cs system
- ✅ Works with PlayerStats speed modifiers
- ✅ No modifications required to existing code
- ✅ Follows existing audio patterns (similar to WateringAudio.cs)