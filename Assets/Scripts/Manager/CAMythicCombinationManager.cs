using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class CAMythicCombinationManager : CAManagerBase<CAMythicCombinationManager>
{
    public struct CombinationRecipe
    {
        public string Key { get; private set; }
        public Dictionary<(CAUnitGrade, CAUnitType), int> requiredUnits;

        public CombinationRecipe(string key, Dictionary<(CAUnitGrade, CAUnitType), int> requirements)
        {
            Key = key;
            requiredUnits = requirements;
        }

        public bool CanCombine(Dictionary<(CAUnitGrade, CAUnitType), int> ownedUnits)
        {
            foreach (var requirement in requiredUnits)
            {
                if (!ownedUnits.ContainsKey(requirement.Key) || ownedUnits[requirement.Key] < requirement.Value)
                {
                    return false;
                }
            }
            return true;

        }
    }

    private Dictionary<string, CombinationRecipe> mythicCombinationRecipes = new Dictionary<string, CombinationRecipe>();

    public override IEnumerator CAAwake()
    {
        mythicCombinationRecipes.Add(CAConstants.SELECT_MYTHIC_01, new CombinationRecipe(CAConstants.SELECT_MYTHIC_01, new Dictionary<(CAUnitGrade, CAUnitType), int>
        {
            { (CAUnitGrade.Normal, CAUnitType.Type1), 1 },
            { (CAUnitGrade.Rare, CAUnitType.Type1), 1 },
            { (CAUnitGrade.Hero, CAUnitType.Type1), 1 }
        }));

        mythicCombinationRecipes.Add(CAConstants.SELECT_MYTHIC_02, new CombinationRecipe(CAConstants.SELECT_MYTHIC_02, new Dictionary<(CAUnitGrade, CAUnitType), int>
        {
            { (CAUnitGrade.Normal, CAUnitType.Type2), 1 },
            { (CAUnitGrade.Rare, CAUnitType.Type2), 1 },
            { (CAUnitGrade.Hero, CAUnitType.Type2), 1 }
        }));
        return base.CAAwake();
    }

    public List<(CAUnitGrade, CAUnitType)> GetOwnedUnitsForMythicCombination(string mythicKey, Dictionary<(CAUnitGrade, CAUnitType), int> ownedUnits)
    {
        List<(CAUnitGrade, CAUnitType)> ownedList = new List<(CAUnitGrade, CAUnitType)>();

        if (mythicCombinationRecipes.ContainsKey(mythicKey))
        {
            CombinationRecipe recipe = mythicCombinationRecipes[mythicKey];
            foreach (var req in recipe.requiredUnits)
            {
                var unitKey = req.Key;
                int requiredCount = req.Value;
                if (ownedUnits.TryGetValue(unitKey, out int currentCount) && currentCount >= requiredCount)
                {
                    ownedList.Add(unitKey);
                }
            }
        }
        return ownedList;
    }
}