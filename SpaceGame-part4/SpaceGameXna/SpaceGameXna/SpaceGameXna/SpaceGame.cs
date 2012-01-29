using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGameTutorial
{
    public class SpaceGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Vaisseau
		Sprite ship;

        // Enemies
        List<Sprite> robots;

        // Compteur pour ajouter des enemies
        long spawnTime;

        // Laser
        Sprite laser;

        // Fond du jeu
        Texture2D backgroundSpace;

        // Fin du jeu ?
        bool finish;

        public SpaceGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 640;
            graphics.PreferredBackBufferHeight = 480;
            graphics.IsFullScreen = false;

            Window.Title = "Tutoriel XNA 03 : Space Game";
			
			robots = new List<Sprite>();
        }

        protected override void Initialize()
        {
			ship = new Sprite(this);
			ship.Speed = new Vector2(2, 2);
			laser = new Sprite(this);
			laser.Speed = new Vector2(4, 4);
            robots.Clear();
            spawnTime = 0;
            finish = false;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Fond de l'écran
            backgroundSpace = Content.Load<Texture2D>("background-space");

            // Texture du vaisseau
			ship.LoadContent("ship/spaceship");

            // Position initiale du vaisseau
            ship.Position = new Vector2(
                (graphics.PreferredBackBufferWidth / 2) - ship.Width / 2,
                graphics.PreferredBackBufferHeight - ship.Height * 2);
			
			laser.LoadContent("laser/laserrouge");
             
            base.LoadContent();
        }

        private void SpawnRobot()
        {
            // Nouveau robot
            Sprite robot = new Sprite(this);

            // Suivant la valeur la valeur aléatoire on charge une texture plutôt qu'une autre
            // Pour diversifier les enemies
            Random rand = new Random();
            if (rand.Next(3) % 2 == 0)
                robot.LoadContent("robot/robotnormal");
            else
                robot.LoadContent("robot/spacealien");

            // On lui attribut une position aléatoire sur X
            // Pour ne pas qu'il sorte de l'écran à droite la valeur max de random doit être
            // égale à la taille de l'écran - la largeur de la texture du robot
            int posX = rand.Next(graphics.PreferredBackBufferWidth - robot.Width);
            // Le robot n'est pas visible et descent du haut vers le bas
            robot.Position = new Vector2(posX, -robot.Height);

            // Plusieurs vitesse de défilement son possibles
            if (rand.Next(6) % 5 == 0)
                robot.Speed = new Vector2(0, 4);
            else
                robot.Speed = new Vector2(0, 2);

            robots.Add(robot);
        }

        private void shoot()
        {
            if (!laser.Active)
            {
                // La position du laser est définie par :
                // - la position en X du vaisseau + la moitier de sa taille
                // - moins la moitier de la taille du laser
                laser.Position = new Vector2(
                    (ship.Position.X + (ship.Width / 2)) - laser.Width / 2, 
                    ship.Position.Y);
				laser.Active = true; // Le laser est tiré !
            }
        }

        // Libération des ressources
        protected override void UnloadContent()
        {
			backgroundSpace.Dispose();
            ship.UnloadContent();
            laser.UnloadContent();

            foreach (Sprite robot in robots)
                robot.UnloadContent();

            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            // On quitte le jeu
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            #region Mise à jour des rectangles du vaisseau et du laser
            ship.Update(gameTime);
            laser.Update(gameTime);
            #endregion

            #region Mise à jour des enemies
            // Toutes les 5 secondes on ajoute un robot
            spawnTime += gameTime.ElapsedGameTime.Milliseconds;
            if (spawnTime > 1000)
            {
                SpawnRobot();
                spawnTime = 0;
            }

            // Pour chaque robot dans la collection
            foreach (Sprite robot in robots)
            {
				// On met à jour le rectangle de chaque robot
				robot.Update(gameTime);
				
                // Si le robot sort de l'écran on ne l'affiche plus et non le met plus à jour
                if (robot.Position.Y >= graphics.PreferredBackBufferHeight)
                    robot.Active = false;
                else
                    robot.Position = new Vector2(robot.Position.X, robot.Position.Y + robot.Speed.Y);
				
				#region Mise à jour des collisions
				// Si un robot touche le vaisseau on arrete tout et on réinitialise l'écran
                if (robot.Rectangle.Intersects(ship.Rectangle))
                    finish = true;	
				
				// Si un laser touche un robot il disparait
				if (laser.Rectangle.Intersects(robot.Rectangle))
					robot.Active = false;	
				#endregion
            }
            #endregion

            #region Mise à jour du laser du vaisseau
            // Si le laser n'est plus visible et qu'il a été tiré
            // On ne le met plus à jour
            if (laser.Active && laser.Position.Y + laser.Height <= 0)
                laser.Active = false; // On peut maintenant retirer un autre laser

            // Si le laser a été tiré on le fait monter
            if (laser.Active)
                laser.Position = new Vector2(laser.Position.X, laser.Position.Y - laser.Speed.Y);

            // Tire le laser
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                shoot();
            #endregion
			
            #region Mise à jour du vaisseau
            // Gestion de l'accélération du vaisseau
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                ship.Speed = new Vector2(5, 5);
            else
                ship.Speed = new Vector2(2, 2);

            // Déplacement du vaisseau et gestion des collisions avec les bords
            if (Keyboard.GetState().IsKeyDown(Keys.Up) && ship.Position.Y > 0)
                ship.Position = new Vector2(ship.Position.X, ship.Position.Y - ship.Speed.Y);

            if (Keyboard.GetState().IsKeyDown(Keys.Down) && ship.Position.Y + ship.Height < graphics.PreferredBackBufferHeight)
                ship.Position = new Vector2(ship.Position.X, ship.Position.Y + ship.Speed.Y); 

            if (Keyboard.GetState().IsKeyDown(Keys.Left) && ship.Position.X > 0)
                ship.Position = new Vector2(ship.Position.X - ship.Speed.X, ship.Position.Y);

            if (Keyboard.GetState().IsKeyDown(Keys.Right) && ship.Position.X + ship.Width < graphics.PreferredBackBufferWidth)
                ship.Position = new Vector2(ship.Position.X + ship.Speed.X, ship.Position.Y);
            #endregion

            // Si la partie est terminée on réinitialise le tout
            if (finish)
                Initialize();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Efface l'écran
            graphics.GraphicsDevice.Clear(Color.AliceBlue);

            // Début du mode dessin
            spriteBatch.Begin();

            // On affichage le fond à la position 0, 0
            spriteBatch.Draw(backgroundSpace, Vector2.Zero, Color.White);

            // On affiche le vaisseau à la position définie dans Update()
            ship.Draw(spriteBatch);

            // Si le laser est tiré on le dessine
            laser.Draw(spriteBatch);

            // On dessine chaque robot actif
            foreach (Sprite robot in robots)
                robot.Draw(spriteBatch);

            // Fin du mode dessin
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

