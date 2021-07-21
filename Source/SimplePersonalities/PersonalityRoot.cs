﻿using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace SPM1
{
    /// <summary>
    /// Corresponds to 'rows' within the 'personality type'.
    /// </summary>
    public class PersonalityRoot : Def, IRuleSupplier
    {
        public static List<PersonalityRoot> All
        {
            get
            {
                return _all ??= DefDatabase<PersonalityRoot>.AllDefsListForReading.ListFullCopy();
            }
        }
        private static List<PersonalityRoot> _all;
        public static List<PersonalityRoot> AllHuman
        {
            get
            {
                return _allHuman ??= DefDatabase<PersonalityRoot>.AllDefsListForReading.Where(p => p.type == Type.HumanLike).ToList();
            }
        }
        private static List<PersonalityRoot> _allHuman;
        public static List<PersonalityRoot> AllAnimal
        {
            get
            {
                return _allAnimal ??= DefDatabase<PersonalityRoot>.AllDefsListForReading.Where(p => p.type == Type.Animal).ToList();
            }
        }
        private static List<PersonalityRoot> _allAnimal;

        public Type type = Type.HumanLike;
        public PersonalityDrive drive;
        public string color;
        public List<PersonalityVariant> variants = new List<PersonalityVariant>();
        public List<PersonalityTrait> traits = new List<PersonalityTrait>();
        public List<HediffDef> hediffs = new List<HediffDef>();
        public List<HediffDef> hediffsIfKnown = new List<HediffDef>();

        public PersonalityVariant GetRandomVariant()
        {
            if (variants.Count == 0)
                return null;

            return variants.RandomElement();
        }

        public PersonalityTrait GetRandomCompatibleTrait(PersonalityVariant withVariant, PersonalityTrait except = null)
        {
            if (traits.Count == 0)
                return null;

            var picked = traits.Where(t => t.IsCompatibleWith(withVariant)).RandomElement();
            while (picked == except)
                picked = traits.Where(t => t.IsCompatibleWith(withVariant)).RandomElement();
            return picked;
        }

        public PersonalityTrait GetRandomCompatibleTrait(PersonalityVariant withVariant, HashSet<PersonalityTrait> except)
        {
            if (traits.Count == 0)
                return null;

            var picked = traits.Where(t => t.IsCompatibleWith(withVariant)).RandomElement();
            while (except.Contains(picked))
                picked = traits.Where(t => t.IsCompatibleWith(withVariant)).RandomElement();
            return picked;
        }

        public IEnumerable<Rule> GetRules(string prefix, Pawn pawn)
        {
            foreach (var item in GrammarUtility.RulesForDef(prefix, this))
                yield return item;

            prefix += '_';
            yield return new Rule_String(prefix + "labelCol", $"<color={Settings.ForcedTraitColor ?? color}>{label}</color>");
        }

        public override void PostLoad()
        {
            base.PostLoad();
            label ??= defName;
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            foreach(var item in variants)
            {
                if (item == null)
                    continue;

                if (item.Root != null)
                {
                    Core.Error($"P.Variant '{item}' is part of more than one personality root! This is not allowed!");
                    continue;
                }
                item.Root = this;
            }

            foreach (var item in traits)
            {
                if (item == null)
                    continue;

                if (item.Root != null)
                {
                    Core.Error($"P.Trait '{item}' is part of more than one personality root! This is not allowed!");
                    continue;
                }
                item.Root = this;
            }
        }

        public enum Type
        {
            HumanLike,
            Animal
        }
    }
}
