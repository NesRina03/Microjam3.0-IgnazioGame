# Game Design Document

## Project Overview

**Working Title:** MicroJam 3.0

**Genre:** First-person puzzle horror / escape room

**Perspective:** First-person

**Platform:** PC

**Engine:** Unity

**Document Version:** 1.0

**Date:** 30 April 2026

## High Concept

The game is a tense first-person puzzle experience set in a hostile laboratory or industrial complex. The player must move through an unstable environment, solve a series of logic-based puzzles, manage limited time, and unlock deeper areas before the creature or system failure catches up with them.

The core tension comes from combining environmental pressure with puzzle solving. Every successful puzzle extends survival time, while every stage slowly drains the clock. The player is constantly deciding whether to solve the current puzzle quickly, spend time on a hint, or push forward to the next objective.

## Design Pillars

1. **Pressure Through Time**
   The player is always under a time constraint. Time is not just a fail state; it is also the reward economy.

2. **Readable, Hands-On Puzzles**
   Puzzles are direct and tactile. The player interacts with terminals, boards, and code inputs from first person.

3. **Escalating Tension**
   The game starts with simpler puzzle interactions and moves toward a higher-stakes second stage with different goals and stronger narrative payoff.

4. **Minimal Friction, Strong Feedback**
   Interactions should feel immediate. Prompts, colors, and audio feedback clearly indicate what can be used and whether the player is making progress.

## Intended Player Experience

The player should feel like they are trapped in a deteriorating facility where every interaction matters. The ideal emotional arc is:

- curiosity at the start,
- urgency as the timer becomes visible,
- relief when a puzzle is solved,
- and final escalation once the second stage is reached.

## Core Gameplay Loop

1. Explore the room or section in first person.
2. Look at a terminal or puzzle object.
3. Press `E` to open the associated puzzle.
4. Solve the puzzle using logic, observation, or code entry.
5. Gain time or unlock progression when the puzzle is solved.
6. Repeat until the required number of puzzles is completed.
7. Transition to Level 2 and reach the final win condition.

## Game Structure

### Main Menu

- Start Game
- Options
- Quit

### Playing State

The active gameplay state where the player can move, inspect terminals, solve puzzles, and manage time.

### Pause State

The game can be paused with `Escape`. Pause should suspend gameplay, hide interactive input, and keep the player from accidentally interacting with puzzle UI.

### Level 2 State

Reached after the required number of puzzles are solved. This state represents the second phase of the game, with its own presentation and endgame progression.

### Win State

Triggered by completing the final objective, such as unlocking the door with the correct code.

### Lose State

Triggered when time runs out or the creature reaches the player, depending on the final balance and scene setup.

## Core Systems

### 1. Time and Instability System

The game uses a stage-based timer system.

#### Rules

- Each stage starts with a base time of 60 seconds.
- Solving a puzzle grants additional time.
- Hints cost time.
- The player loses if the timer reaches zero before completing the stage chain.

#### Purpose

This system creates pressure without forcing the game into pure speedrun territory. The player can recover time through skillful play, but careless hint usage or slow puzzle solving will eventually create failure.

#### Gameplay Impact

- Encourages efficient puzzle solving.
- Makes hints meaningful.
- Reinforces the horror/survival tone.

### 2. Puzzle Interaction System

Puzzle interaction is based on looking at a target object and pressing `E`.

#### Behavior

- A centered raycast checks what the player is looking at.
- If the object is a valid puzzle terminal, a prompt appears.
- Pressing `E` opens the puzzle UI.
- Door interactions are restricted to Level 2.

#### Feedback

- Prompt text appears on screen.
- The selected object can be highlighted or marked solved.
- Puzzle panels take over the screen when opened.

### 3. Wordle-Style Puzzle

The game includes a Wordle-inspired terminal puzzle.

#### Structure

- 6 rows
- 5 letters per row
- Keyboard input and UI button input supported

#### Rules

- The player enters a 5-letter guess.
- Correct letters are shown in green.
- Present but misplaced letters are shown in yellow.
- Incorrect letters are shown in grey.

#### Hint System

- The hint button reveals 2 letters.
- The hint costs 15 seconds of total game time.
- Hints are only available if enough time remains.

#### Purpose

This puzzle serves as a fast, readable logic challenge that can be reused for different environmental factors.

### 4. Pigpen Puzzle

The Pigpen puzzle is a second, more diegetic code-based puzzle.

#### Current Behavior

- The player approaches the board in the scene.
- A prompt appears when the board is in range.
- The player can press `E` to open the puzzle.
- The exit button closes the puzzle.

#### Puzzle Check String

The answer string is:

`twntysxzerofour`

#### Role in the Game

This puzzle adds variety by shifting from letter deduction to direct text entry and recognition.

### 5. Door Code Puzzle

The final gate uses a 4-digit code input.

#### Correct Code

`2604`

#### Purpose

The door puzzle acts as a final lock or endgame gate. It is simple enough to resolve quickly after the player has learned the rhythm of the game, but still serves as a strong finish to the second stage.

### 6. Audio and Options System

The game includes volume control for music and sound effects.

#### Features

- Music slider
- SFX slider
- AudioMixer-based routing
- PlayerPrefs saving

#### Design Goal

Audio should support the atmosphere without overwhelming puzzle readability. Music should intensify the tension, while UI and interaction sounds provide immediate feedback.

## Controls

### Exploration

- `WASD` or arrow keys: move
- Mouse: look around
- `E`: interact
- `Escape`: pause / close applicable UI

### Puzzle UI

- Mouse: click buttons
- Keyboard: type answers or digits where supported
- `Enter`: submit in text-entry puzzles
- `Backspace`: delete a character

## Level Flow

### Level 1

The player begins in the first playable space. This level introduces the core puzzle loop and time pressure.

#### Goals

- Discover interactive terminals.
- Solve the required puzzle set.
- Manage time carefully.
- Learn the interaction language of the game.

#### Tone

The first level should feel oppressive but understandable. It is the tutorial phase for the larger mechanics.

### Transition to Level 2

Once the puzzle quota is reached, the game transitions to Level 2.

#### Expected Changes

- New UI/state presentation.
- Level 2 music.
- Access to the next objective set.
- Door interaction becomes available only here.

### Level 2

The second stage raises stakes and shifts the player toward the end condition.

#### Goals

- Solve the additional puzzle present in the scene.
- Enter the door code.
- Reach the win state.

## Narrative Direction

The current asset and system setup suggests a laboratory or facility under failure, with the player trying to survive while uncovering the correct sequence of actions to escape or stabilize the environment.

Suggested narrative themes:

- containment failure,
- corrupted systems,
- experimental danger,
- and an unseen presence or creature driving the urgency.

The story should be told through environment, UI language, puzzle terminals, and scene progression rather than heavy exposition.

## Art Direction

### Visual Tone

The game should feel industrial, claustrophobic, and slightly decayed. Lighting should emphasize contrast and narrow visibility.

### Asset Language

- Metal surfaces
- Lab equipment
- Terminal screens
- Warning lights
- Framed boards and control panels

### UI Style

The UI should be clear, high-contrast, and readable. Puzzle panels should feel like they belong inside the facility rather than as detached overlays.

## Audio Direction

### Music

Music should support atmosphere and progression:

- calmer tension in early play,
- stronger presence in Level 2,
- clear win/lose transitions.

### Sound Effects

- button clicks,
- puzzle confirmations,
- time pressure cues,
- interaction prompts,
- failure and success feedback.

### Mixing

The mix should keep UI and puzzle sounds audible over ambient music so the player never misses critical feedback.

## UI and UX Requirements

### Prompting

- Show a prompt when the player can interact.
- Use distinct color or emphasis for actionable targets.

### Feedback

- Clearly indicate when a puzzle is active.
- Clearly indicate when a puzzle is solved.
- Show time penalties and hint usage feedback.

### Responsiveness

- Pressing `E` should open the expected puzzle immediately.
- Exit buttons should close puzzle UIs reliably.
- The player should not feel stuck in a panel after solving or backing out.

## Technical Design

### Major Managers

- `GameManager`: game state, screen flow, pause, win/lose, Level 2 transition.
- `PuzzleManager`: puzzle activation, solved tracking, panel control.
- `InstabilityManager`: stage timing, time rewards, progression.
- `MusicManager`: music playback and transitions.
- `AudioSettings`: volume loading and mixer control.

### Puzzle Implementation Pattern

Puzzles are scene objects with attached controllers and UI panels. Each puzzle should be easy to wire in the Inspector and should fail safely if a panel reference is missing.

### Interaction Pattern

The interaction raycaster is the central world-to-puzzle bridge. It detects targets in the center of the screen and opens the proper UI state.

### State Safety

The game should always recover from missing references by restoring player control rather than freezing.

## Content List

### Implemented Puzzle Types

- Wordle-style letter puzzle
- Pigpen text-entry puzzle
- Door code puzzle

### Implemented Environmental Factor Panels

- light
- temp
- pressure
- oxygen
- radiation

### Planned / Scene-Driven Content

- additional terminals or level variations,
- alternate puzzle arrangements,
- stronger narrative props,
- and optional balancing updates.

## Balancing Notes

### Timer Balance

The 60-second stage timer is intentionally tight. Reward time must be noticeable but not so generous that pressure disappears.

### Hint Balance

Hints should feel expensive enough that the player thinks before using them.

### Puzzle Density

The game should avoid placing too many long puzzles back-to-back. The player needs recovery moments between pressure spikes.

## Accessibility Considerations

- Ensure text is readable at standard PC resolutions.
- Keep prompts high contrast.
- Allow keyboard entry for puzzles where practical.
- Provide clear success/failure messaging.

## Risks and Open Issues

- Scene wiring must remain consistent when merging content from different scene versions.
- Puzzle panels should be checked for canvas scaling across resolutions.
- Audio mixer assignments must be validated in every scene.
- Parent/child hierarchy differences can break interaction if not handled carefully.

## Future Enhancements

- More puzzle variants tied to environmental factors.
- Additional narrative clues and collectibles.
- A more dynamic creature or chase system.
- Stronger transition presentation between levels.
- Better puzzle-specific animations and screen effects.

## Summary

This game is a compact first-person puzzle horror experience built around time pressure, readable interaction, and escalating challenge. Its strength comes from the combination of immediate feedback, puzzle variety, and a clear progression from a tense first stage into a more focused final objective.

The current implementation already supports a solid core loop. The next step in production would be to refine level content, tune pacing, and expand the narrative presentation around the existing mechanics.
