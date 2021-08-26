
// On cr�e un namespace � part pour les enums globaux qui peuvent �tre utilis�s par diff�rents syst�mes
// sans avoir � les rendre d�pendants les uns des autres
namespace Project.Enums
{

    /// <summary>
    /// Indique si la transition du ScreenFader se fait en fondu ou via un masque modifi� par shader
    /// </summary>
    public enum FaderTransitionType
    {
        Mask = 0,   //Utilise une texture pour masquer le ui
        ColoredFade = 1, //Fondu en couleur vers la sc�ne ou la texture suivante
        TextureBlend = 2,   //Fondu en transparence vers la texture suivante
    }
}


