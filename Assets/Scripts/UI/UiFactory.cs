using System;
using UnityEngine;
using UnityEngine.UIElements;
using UIP.Core;

namespace UIP.UI
{
    public static class UiFactory
    {
        public static Label Title(string text, string className = "header-title")
        {
            var label = new Label(text);
            label.AddToClassList(className);
            return label;
        }

        public static Label Muted(string text)
        {
            var label = new Label(text);
            label.AddToClassList("muted");
            label.style.whiteSpace = WhiteSpace.Normal;
            return label;
        }

        public static Label Body(string text, string className = "card-body")
        {
            var label = new Label(text);
            label.AddToClassList(className);
            label.style.whiteSpace = WhiteSpace.Normal;
            return label;
        }

        public static Button Primary(string text, Action onClick)
        {
            var button = new Button(onClick) { text = text };
            button.AddToClassList("btn");
            return button;
        }

        public static Button Secondary(string text, Action onClick)
        {
            var button = Primary(text, onClick);
            button.AddToClassList("btn-secondary");
            return button;
        }

        public static Button Small(string text, Action onClick, bool accent = false)
        {
            var button = new Button(onClick) { text = text };
            button.AddToClassList("btn");
            button.AddToClassList("btn-small");
            if (!accent)
            {
                button.AddToClassList("btn-secondary");
            }

            return button;
        }

        public static VisualElement Card()
        {
            var card = new VisualElement();
            card.AddToClassList("card");
            return card;
        }

        public static VisualElement RowSpread()
        {
            var row = new VisualElement();
            row.AddToClassList("row-spread");
            return row;
        }

        public static Label Chip(string text, bool accent = false)
        {
            var chip = new Label(text);
            chip.AddToClassList("chip");
            if (accent)
            {
                chip.AddToClassList("chip-accent");
            }

            return chip;
        }

        public static Label DifficultyBadge(Difficulty difficulty)
        {
            var badge = new Label(ScoreUtil.DifficultyLabel(difficulty));
            badge.AddToClassList("badge");
            switch (difficulty)
            {
                case Difficulty.Beginner:
                    badge.AddToClassList("badge-beginner");
                    break;
                case Difficulty.Junior:
                    badge.AddToClassList("badge-junior");
                    break;
                case Difficulty.Mid:
                    badge.AddToClassList("badge-mid");
                    break;
                case Difficulty.Senior:
                    badge.AddToClassList("badge-senior");
                    break;
                default:
                    badge.AddToClassList("badge-lead");
                    break;
            }

            return badge;
        }

        public static VisualElement ProgressBar(float value01)
        {
            var track = new VisualElement();
            track.AddToClassList("progress-track");
            var fill = new VisualElement();
            fill.AddToClassList("progress-fill");
            fill.style.width = Length.Percent(Mathf.Clamp01(value01) * 100f);
            track.Add(fill);
            return track;
        }

        public static ScrollView Scroll()
        {
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.AddToClassList("scroll");
            scroll.style.flexGrow = 1;
            return scroll;
        }

        public static TextField SearchField(string placeholder, Action<string> onChanged)
        {
            var field = new TextField { value = "" };
            field.AddToClassList("field");
            field.label = "";
            try
            {
                field.textEdition.placeholder = placeholder;
            }
            catch
            {
                // Unity version variance for placeholder API.
            }

            field.RegisterValueChangedCallback(evt => onChanged?.Invoke(evt.newValue));
            return field;
        }

        public static VisualElement StatCell(string value, string label)
        {
            var cell = new VisualElement();
            cell.AddToClassList("stat-cell");
            var v = new Label(value);
            v.AddToClassList("stat-value");
            var l = new Label(label);
            l.AddToClassList("stat-label");
            cell.Add(v);
            cell.Add(l);
            return cell;
        }

        public static VisualElement Section(string title)
        {
            var label = new Label(title);
            label.AddToClassList("section-title");
            return label;
        }

        public static Label Code(string text)
        {
            var label = new Label(text);
            label.AddToClassList("code-block");
            label.style.whiteSpace = WhiteSpace.Normal;
            return label;
        }
    }
}
