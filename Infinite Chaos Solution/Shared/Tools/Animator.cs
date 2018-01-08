
using System;
using System.Xml;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Shared.Tools
{
    class FrameData
    {
        //Animation ID, used when refering to a specific animation
        public int ID;

        //Data for the current frame of the sprite sheet (x1, y1, width, height)
        public Rectangle rect = new Rectangle();

        //Data for the pivot point of the current frame
        public Vector2 pivot = new Vector2();
    };



    public class Animator
    {
        public enum Animations
        { Run, Idle, Attack }

        private Dictionary<Animations, int>_loadedAnimations = new Dictionary<Animations,int>();

        private Game _game;

        //-------------------------------Declarations------------------------------//

        public Animations CurrentAnimation { get; private set; }
        public float m_animationTimer;
        public int m_currentFrame;

        

        public int m_animationSpeed;

        Texture2D m_currentSheet;

        public bool PlayingPlayOnceAnimation { get; private set; }

        Dictionary<int, Texture2D> m_animations;

        XmlDocument m_currentDoc = new XmlDocument();

        //2D list of all of the animation data for this sprite
        List<List<FrameData>> m_frameData;

        //------------------------------------------------------------------------//


        //Default constructor, takes in a reference to which sprite this animator belongs to
        public Animator(Game game)
        {
            _game = game;
            m_animationSpeed = 10;
            m_animationTimer = 0.3F;
            m_frameData = new List<List<FrameData>>();
            m_animations = new Dictionary<int, Texture2D>();
        }

        //Adds an animation to this animator in the form of an xml document, returns the ID of this animation as an int
        public void AddAnimation(string doc, string sheet, Animations animation)
        {
#if PSM
			doc = doc.Insert(0,@"Application\");
#endif
			
            //Saves the sprite sheet to memory
            m_animations.Add(m_frameData.Count, _game.Content.Load<Texture2D>(sheet));

            XmlDocument newDoc = new XmlDocument();
            newDoc.Load(doc);

            //Sets the path to the first frame
            XmlNode path = newDoc.SelectSingleNode("MySheet/sprite");

            //Data for this animation
            List<FrameData> newList = new List<FrameData>();

            while (path != null)
            {
                FrameData newData = new FrameData();

                //Read in the values from the xml document into the animation data
                newData.ID = Convert.ToInt16(path.Attributes[0].Value);
                newData.rect.X = Convert.ToInt16(path.Attributes[1].Value);
                newData.rect.Y = Convert.ToInt16(path.Attributes[2].Value);
                newData.rect.Width = Convert.ToInt16(path.Attributes[3].Value);
                newData.rect.Height = Convert.ToInt16(path.Attributes[4].Value);
                newData.pivot.X = newData.rect.Width * Convert.ToSingle(path.Attributes[5].Value);
                newData.pivot.Y = newData.rect.Height * Convert.ToSingle(path.Attributes[6].Value);

                //Save this frame to the new animation
                newList.Add(newData);

                //Move onto the next frame
                path = path.NextSibling;
            }

            //Save this animation into memory
            m_frameData.Add(newList);


            _loadedAnimations[animation] =  m_frameData.Count - 1;
        }



        /// <summary>
        /// Sets the current animation to whatever animation ID is passed in.
        /// 'playOnce' animations get priority and will play through to the end.
        /// </summary>
        public void SetAnimation(Animations _animation, bool playOnce = false)
        {
            if(!_loadedAnimations.ContainsKey(_animation))
                throw new Exception("Animation not loaded!");

            int animation = _loadedAnimations[_animation];

            // Do nothing, if trying to set a non-play-once animation while a play-once animation is still playing.
            if (PlayingPlayOnceAnimation && !playOnce) return;

            // Do nothing, if trying to set a non-play-once animation that's already set.
            if (CurrentAnimation == _animation && !PlayingPlayOnceAnimation) return;


            if (playOnce)
                // Set this animation to play to the end. (Note: interrupted if another 'play-once' animation set.) //
                PlayingPlayOnceAnimation = true;


            //Change the animation if it isnt already this animation
            CurrentAnimation = _animation;
            m_currentFrame = 0;
            m_currentSheet = m_animations[animation];
        }

        //Sets the animation fps speed to speed
        public void SetAnimationSpeed(int speed)
        {
            m_animationSpeed = speed;
        }

        //Updates this animator
        public void Update(GameTime gameTime)
        {
            if (m_frameData.Count == 0) return;

            m_animationTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Increments the frame for the current animation
            if (m_animationTimer < 0)
            {
                m_currentFrame++;

                // if 'play-once' animation has reached it's end.
                if (PlayingPlayOnceAnimation && m_currentFrame > m_frameData[_loadedAnimations[CurrentAnimation]].Count - 1)
                {
                    // allow non'set-once' animations to play.
                    PlayingPlayOnceAnimation = false;
                    m_currentFrame--;
                }
                else if (m_currentFrame > m_frameData[_loadedAnimations[CurrentAnimation]].Count - 1)
                    m_currentFrame = 0;

                //Reset the animation timer
                m_animationTimer += 1.0f / (float)m_animationSpeed;
            }
        }

        //Returns the duration of the current animation implying the animation speed does not change
        public float GetDuration()
        {
            return m_frameData[_loadedAnimations[CurrentAnimation]].Count * (1.0f / (float)m_animationSpeed);
        }

        //returns the active UV coordinates of the sheet
        public Rectangle GetActiveRect()
        {
            if (m_frameData.Count != 0)
                return m_frameData[_loadedAnimations[CurrentAnimation]][m_currentFrame].rect;
            else
                return new Rectangle(0, 0, 128, 128);
        }

        public Vector2 GetPivotVec()
        {
            if (m_frameData.Count != 0)
                return m_frameData[_loadedAnimations[CurrentAnimation]][m_currentFrame].pivot;
            else
                return new Vector2(0, 0);
        }

        public Texture2D GetActiveTexture()
        {
            if (m_frameData.Count == 0)
                throw new Exception("Animation was not found");

            return m_animations[_loadedAnimations[CurrentAnimation]];

        }
    }
}
