# App Store Review Notes — Unity Interview Prep

**Bundle ID:** `com.goldbox.uip`  
**Developer:** Gold Box  
**Build focus:** Offline-first interview preparation for Unity developers

## What makes this app different (Guideline 4.2 / 4.3)

This is not a static FAQ wrapper. The product is built around a learning loop:

1. **Learn** — role-based paths (Beginner → Technical Lead, Mobile, AR/VR) with original lessons
2. **Practice** — 100+ original interview prompts with interviewer intent, ideal answers, mistakes, follow-ups, difficulty
3. **Test** — timed mock interviews (2–3 minute think timers, reveal, self-rating, resume interrupted sessions)
4. **Improve** — weak-topic ranking, confidence tracking, daily streaks, bookmarks, spaced-repetition flashcards, dashboard stats

Interactive state is persisted locally and drives recommendations on Home.

## Offline & privacy

- Fully functional offline
- No account system
- No ads
- No third-party analytics SDKs in this MVP
- Progress stored only in app sandbox (`Application.persistentDataPath`)

## Review navigation

1. Launch → Splash / onboarding disclaimer
2. Home → Continue card, weak-topic recommendation, mock CTA
3. Learn → open a path → mark a module complete
4. Practice → filter by topic/difficulty → reveal answer → self-rate
5. Mock → start 5-question session → reveal → rate → summary
6. Flashcards → review due cards with Again/Hard/Good
7. Progress → stats + weak topics
8. Settings → About / Disclaimer / Privacy / reset

## Content ownership

All interview prompts, explanations, flashcards, lessons, and code sketches are **original educational writing** created for this app. They are **not** copied from:

- Unity Manual
- Unity Scripting API reference text
- Unity Learn tutorials
- third-party blog Q&A pages verbatim

Facts about public APIs are explained in original wording with original examples.

## Trademark / naming notice (material risk)

Product name currently: **Unity Interview Prep**

In-app disclaimer (About / onboarding):

> Unity is a trademark of Unity Technologies. This application is an independent educational resource and is not affiliated with, endorsed by, or sponsored by Unity Technologies.

**Unresolved risk:** Apple App Review Guideline **5.2(c)** and Unity trademark guidelines may still object to using “Unity” in the **app name** without permission, even with a disclaimer. If App Review requests a rename, proposed alternatives include distinctive independent names with descriptive subtitle copy only.

No Unity logos, editor screenshots, package icons, or Unity Learn artwork are used. Branding assets are original.

## Suggested App Store metadata (draft)

- **Name:** Unity Interview Prep *(subject to trademark/review outcome)*
- **Subtitle:** 700+ Questions & Practice *(MVP ships ~100 curated questions with schema for expansion)*
- **Category:** Education
- **Keywords:** interview, game developer, csharp, coding practice, flashcards *(avoid packing unrelated trademark spam)*

## Contact for review

Provide your Apple Review contact notes / demo account fields as applicable. No login is required for this MVP.
