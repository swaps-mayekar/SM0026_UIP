# Unity Interview Prep (MVP)

Offline-first Unity interview preparation app for iOS (portrait), built in Unity 6.

## Features

- **Learn** — 7 role-based learning paths with original lessons
- **Practice** — 100+ original interview questions (intent, ideal answer, mistakes, follow-ups, difficulty)
- **Test** — timed mock interviews with pause/reveal/self-rate and resume
- **Improve** — streaks, weak topics, confidence, bookmarks, spaced-repetition flashcards
- **Common mistakes** — interviewer expectations and better alternatives
- Local-only persistence (no ads/analytics/accounts in MVP)

## Open in Editor

1. Open with Unity **6000.3.19f1**
2. Open scene `Assets/Scenes/0_SplashScene.unity`
3. Press Play

Optional menu items:

- `UIP/Setup MVP Scene`
- `UIP/Validate MVP`

## Tests

```bash
/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath . \
  -runTests -testPlatform EditMode -testResults Logs/EditModeResults.xml -logFile Logs/editmode.log
```

## Legal

See `Docs/Disclaimer.md`, `Docs/PrivacySummary.md`, and `Docs/AppStoreReviewNotes.md`.

**Trademark note:** the product name includes “Unity” and remains a review/trademark risk despite the in-app independent-resource disclaimer.
