using System;
using System.Collections.Generic;
using UnityEngine;
using static Project.Utilities.ValueTypes.Enums;

namespace Project.NodeSystem 
{

    [CreateAssetMenu(menuName = "Dialogue/New Dialogue Character", fileName = "New Dialogue Character")]
    [System.Serializable]
    public class DialogueCharacterSO : ScriptableObject
    {
        public List<string> characterNames = new List<string>(); //Les noms traduits dans toutes les langues disponibles
        public Color characterNameColor = Color.white;
        [SerializeField] private List<FaceAndMood> faces = new List<FaceAndMood>();


        private void OnValidate()
        {
            int length = LengthOf<LanguageType>();

            //Initialiser les noms du perso quand on cr�e le SO pour la premi�re fois ou qu'on le r�initialise.
            //Normalement le perso a tjs le m�me nom, mais s'il est �crit dans un alphabet diff�rent (latin, cyrillique, mandarin, etc.),
            //Cela nous permet de changer les caract�res.
            if (characterNames == null ^ characterNames.Count < length)
            {
                for (int i = characterNames.Count; i < length; i++)
                {
                    characterNames.Add(string.Empty);
                }
            }


            length = LengthOf<CharacterMood>();

            //Initialiser les sprites et enums quand on cr�e le SO pour la premi�re fois ou qu'on le r�initialise.
            if (faces == null ^ faces.Count < length)
            {
                for (int i = faces.Count; i < length; i++)
                {
                    faces.Add(new FaceAndMood { face = null, mood = (CharacterMood)i });
                }
            }
        }



        public FaceAndMood GetFaceAndMood(int index)
        {
            return faces[index];
        }


        public Sprite GetFaceFromMood(CharacterMood mood)
        {
            return faces[(int)mood].face;
        }



        
    }

    [System.Serializable]
    public class FaceAndMood
    {
        public Sprite face;
        public CharacterMood mood;
    }
}