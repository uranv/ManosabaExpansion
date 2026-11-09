using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Utils;

[StaticConstructorOnStartup]
public static class NarehateCorpseRecipeBindings
{
    static NarehateCorpseRecipeBindings()
    {
        var humanCorpse = ThingDefOf.Human.race.corpseDef;
        var narehateCorpse = ModDefOf.UmThingCorpseNarehate;
        foreach (var recipe in DefDatabase<RecipeDef>.AllDefs)
        {
            if (recipe.fixedIngredientFilter == null ||
                !recipe.fixedIngredientFilter.AllowedThingDefs.Contains(humanCorpse)) continue;

            recipe.fixedIngredientFilter.SetAllow(narehateCorpse, true);
            if (recipe.defaultIngredientFilter != null)
            {
                recipe.defaultIngredientFilter.SetAllow(narehateCorpse,
                    recipe.defaultIngredientFilter.AllowedThingDefs.Contains(humanCorpse));
            }
            if (recipe.ingredients == null) continue;
            foreach (var ingredient in recipe.ingredients)
            {
                if (ingredient.filter != null && ingredient.filter.AllowedThingDefs.Contains(humanCorpse))
                    ingredient.filter.SetAllow(narehateCorpse, true);
            }
        }
    }
}
