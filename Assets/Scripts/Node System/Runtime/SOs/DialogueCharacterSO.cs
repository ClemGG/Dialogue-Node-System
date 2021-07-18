using System;
using UnityEngine;

namespace Project.NodeSystem 
{

    [CreateAssetMenu(menuName = "Dialogue/New Dialogue Character", fileName = "New Dialogue Character")]
    [System.Serializable]
    public class DialogueCharacterSO : ScriptableObject
    {
        public string characterName;
        public Color characterNameColor = Color.white;
        [SerializeField] private FaceAndMood[] faces;
            
            


        //Initialiser les sprites et enums quand on crée le SO pour la première fois
        private void Reset()
        {
            if(faces == null)
            {
                int length = Enum.GetValues(typeof(CharacterMood)).Length;
                faces = new FaceAndMood[length];

                for (int i = 0; i < length; i++)
                {
                    faces[i] = new FaceAndMood { face = null, mood = (CharacterMood)i };
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