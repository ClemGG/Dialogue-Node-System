
// On crée un namespace à part pour les enums globaux qui peuvent être utilisés par différents systèmes
// sans avoir à les rendre dépendants les uns des autres
namespace Project.Enums
{

    /// <summary>
    /// Indique si la transition du ScreenFader se fait en fondu ou via un masque modifié par shader
    /// </summary>
    public enum FaderTransitionType
    {
        Mask = 0,   //Utilise une texture pour masquer le ui
        ColoredFade = 1, //Fondu en couleur vers la scène ou la texture suivante
        TextureBlend = 2,   //Fondu en transparence vers la texture suivante
    }
}


