# Changelog

## 1.2.1

First GitHub release of the existing mod.

- Separate window control, interface construction, widgets, and rate validation.
- Divide campaign setup into smaller initialization steps.
- Replace numeric identifiers with named constants.
- Keep income rules and the ES3 v1 save format unchanged.
- Include campaign-specific rates, legacy configuration migration, and translations for the game's 16 language columns.
- Make source builds non-deploying by default. Use `-p:DeployPlugin=true` to install after a build.

## 1.2.0

- Move settings from F6 to the new campaign screen.
- Store the enabled state and rates separately in each save.
- Migrate legacy configuration values without changes to the global configuration file.

## 1.1.1

- Simplify the settings window and report text.
- Remove unused translations.

## 1.1.0

- Add localized settings and daily reports.
