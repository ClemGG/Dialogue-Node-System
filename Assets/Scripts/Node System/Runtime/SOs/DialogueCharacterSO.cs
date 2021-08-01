using System.Collections.Generic;
using UnityEngine;
using static Project.Utilities.ValueTypes.Enums;

namespace Project.NodeSystem 
{

    [CreateAssetMenu(menuName = "Dialogue/New Dialogue Character", fileName = "New Dialogue Character")]
    [System.Serializable]
    public class DialogueCharacterSO : ScriptableObject
    {
        public List<string> CharacterNames = new List<string>(); //Les noms traduits dans toutes les langues disponibles
        public Color CharacterNameColor = Color.white;
        public AudioClip CharPrintClip;
        [SerializeField] private List<FaceAndMood> m_faces = new List<FaceAndMood>();



        private void OnValidate()
        {
            int length = LengthOf<LanguageType>();

            //Initialiser les noms du perso quand on crée le SO pour la première fois ou qu'on le réinitialise.
            //Normalement le perso a tjs le même nom, mais s'il est écrit dans un alphabet différent (latin, cyrillique, mandarin, etc.),
            //Cela nous permet de changer les caractères.
            if (CharacterNames == null ^ CharacterNames.Count < length)
            {
                for (int i = CharacterNames.Count; i < length; i++)
                {
                    CharacterNames.Add(string.Empty);
                }
            }


            length = LengthOf<CharacterMood>();

            //Initialiser les sprites et enums quand on crée le SO pour la première fois ou qu'on le réinitialise.
            if (m_faces == null ^ m_faces.Count < length)
            {
                for (int i = m_faces.Count; i < length; i++)
                {
                    m_faces.Add(new FaceAndMood { Face = null, Mood = (CharacterMood)i });
                }
            }
        }



        public FaceAndMood GetFaceAndMood(int index)
        {
            return m_faces[index];
        }


        public Sprite GetFaceFromMood(CharacterMood mood)
        {
            return m_faces[(int)mood].Face;
        }



        
    }

    [System.Serializable]
    public class FaceAndMood
    {
        public Sprite Face;
        public CharacterMood Mood;
    }
}