# Movement Audio System Documentation

## Overview
The MovementAudio system provides footstep sound effects for the player character when moving. It automatically plays footstep sounds while the character is moving and stops when the character is idle.

## Setup Instructions

### 1. Add MovementAudio Component
1. Select the Player GameObject in the hierarchy
2. Add the `MovementAudio` script component
3. The system will automatically find and integrate with the existing `Movement` component

### 2. Configure Audio Settings

#### Footstep Sounds
- **Footstep Sounds Array**: Drag multiple footstep audio files here for variety
- Recommended formats: `.mp3`, `.wav`, `.ogg`
- Place footstep audio files in `Assets/Audio/Footsteps/` folder

#### Audio Configuration
- **Volume** (0-1): Controls the volume of footstep sounds
- **Footstep Interval** (0.1-1 seconds): Time between each footstep sound
- **Enable Footstep Audio**: Toggle to enable/disable the system

#### Debug Options
- **Show Debug Info**: Enable console logging for troubleshooting

### 3. Audio File Organization
```
Assets/
└── Audio/
    ├── BackGroundMusic/
    ├── CuttingTree/
    ├── Watering/
    └── Footsteps/          # New folder for footstep sounds
        ├── footstep1.mp3
        ├── footstep2.mp3
        └── footstep3.mp3
```

## Features

### Automatic Integration
- Automatically detects when player starts/stops moving
- No manual triggering required
- Works with existing Movement.cs system

### Configurable Settings
- Multiple footstep sounds for variation
- Adjustable volume and timing
- Runtime configuration support

### Performance Optimized
- Uses `PlayOneShot()` for non-overlapping sounds
- Timer-based system prevents audio spam
- Minimal performance impact

### Audio Variety
- Randomly selects from available footstep sounds
- Prevents repetitive audio patterns
- Supports any number of footstep variations

## Public Methods

### Runtime Configuration
```csharp
// Get MovementAudio component
MovementAudio audioComponent = GetComponent<MovementAudio>();

// Adjust volume at runtime
audioComponent.SetVolume(0.8f);

// Change footstep timing
audioComponent.SetFootstepInterval(0.3f);

// Enable/disable system
audioComponent.SetFootstepAudioEnabled(false);

// Manual footstep trigger
audioComponent.PlayFootstepSoundManual();
```

### Getting Current Settings
```csharp
float currentVolume = audioComponent.GetVolume();
float currentInterval = audioComponent.GetFootstepInterval();
bool isEnabled = audioComponent.IsFootstepAudioEnabled();
```

## Integration with Movement.cs

The MovementAudio system integrates seamlessly with the existing Movement.cs:

```csharp
// Access MovementAudio from Movement script
Movement movement = GetComponent<Movement>();
MovementAudio audioComponent = movement.GetMovementAudio();
```

## Troubleshooting

### No Sound Playing
1. Check that footstep sounds are assigned in the inspector
2. Verify AudioSource component exists (auto-created if missing)
3. Ensure "Enable Footstep Audio" is checked
4. Check volume settings (both script and AudioSource)

### Performance Issues
1. Reduce number of footstep sound variations
2. Use compressed audio formats (.mp3, .ogg)
3. Increase footstep interval to reduce frequency

### Audio Not Stopping
1. Verify Movement.cs `IsMoving()` method works correctly
2. Check debug console for movement state changes
3. Ensure MovementAudio component is on same GameObject as Movement

## Technical Details

- **Dependencies**: Requires Movement.cs component
- **AudioSource**: Automatically created if not present
- **Update Frequency**: Runs in Update() for responsive audio
- **Memory Usage**: Minimal - uses PlayOneShot for one-time sounds
- **Compatibility**: Works with existing PlayerStats speed modifiers

## Best Practices

1. **Audio Quality**: Use consistent audio format and quality for all footstep sounds
2. **File Naming**: Use descriptive names like `grass_step_01.mp3`, `stone_step_02.mp3`
3. **Volume Balance**: Test footstep volume against background music
4. **Variation**: Include 3-5 different footstep sounds for good variety
5. **Performance**: Keep audio files reasonably small (< 1MB each)