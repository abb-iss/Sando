using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using RRRSRoguelike.Entities.Creatures;
using RRRSRoguelike.Entities.Props;
using RRRSRoguelike.Enums;
using RRRSRoguelike.Helpers;
using RRRSRoguelikeData;


namespace RRRSRoguelike.Managers
{
	/// <summary>
	/// Manages all creatures.
	/// </summary>
	class CreatureManager
	{

		#region variables
		private Player player;
		private bool playerAlive;

		private int newMonsterDelay = 10;
		private int currentNewMonsterDelay;
		private int newMonstersAdded = 0;

		private DungeonManager dungeonManager = DungeonManager.Instance;

		public IList<Monster> Monsters = new List<Monster>();
		
		public delegate void RaiseStatEventHandler(object sender, RaiseStatArgs e);
		public event RaiseStatEventHandler MonsterKilledEvent;
		#endregion

		#region constructors

		public CreatureManager()
		{
			currentNewMonsterDelay = newMonsterDelay;
		}

		#endregion

		#region methods

		public void SetupPlayer(Point position)
		{

			player = new Player(position,
								(TileData)dungeonManager.Dungeon.Data.Players["Player"]);

			player.CreatureDeadEvent += new Creature.RaiseStatHandler(player_CreatureDeadEvent);

			playerAlive = true;
		}

		public void MonsterKilled(object sender, RaiseStatArgs e)
		{
			if (MonsterKilledEvent != null)
				MonsterKilledEvent(sender, e);
		}

		public void ClearMonsters()
		{
			Monsters.Clear();
		}

		public void AddRandomMonstersByLevel()
		{
			//add monsters * level
			//need this for the isoktoplace monster to work

			dungeonManager.CreatureManager.CreaturesToTiles();
			dungeonManager.PropManager.PropsToTiles();

			for (int i = 0; i < Constants.NumberOfMonsters * dungeonManager.CurrentDungeonLevel; i++)
			{

				AddRandomMonster(i, GetRandomMonsterData());
			}

		}

		public TileData GetRandomMonsterData()
		{
			int index = dungeonManager.MonsterRandom.Next(0, dungeonManager.Dungeon.Data.Creatures.Count);
			return dungeonManager.Dungeon.Data.Creatures.ElementAt(index).Value;
		}

		public void AddRandomMonster(int i, TileData tileData)
		{

			//Used this to debug monster movement.
#if DEBUG
        tileData.ImageCharacter = i.ToString().First().ToString();
#endif
			AddMonster(dungeonManager.DungeonLevel.GetValidRandomPoint(true), tileData);

		}

		public void AddMonster(Point position, TileData tileData)
		{
			Monster monster = new Monster(position, tileData);
			monster.CreatureDeadEvent += new Creature.RaiseStatHandler(monster_CreatureDeadEvent);
			monster.CreatureDeadEvent += new Creature.RaiseStatHandler(MonsterKilled);
			Monsters.Add(monster);
			//need this for the isoktoplace monster to work
			dungeonManager.DungeonLevel.AddTile(monster, Layers.Creature);

		}

		public void ProcessMonsters()
		{
			if (Monsters.Count() > 0)
			{
				MoveMonsters();
				if (!DungeonManager.Instance.GodMode)
					CreatureCollisions();
			}
			else
			{
				if (currentNewMonsterDelay < 0)
				{
					AddMonster(dungeonManager.PropManager.Props.OfType<Stairwell>().FirstOrDefault().Position,
							   GetRandomMonsterData());
					newMonstersAdded++;
					currentNewMonsterDelay = Math.Max(0,
							   newMonsterDelay - dungeonManager.CurrentDungeonLevel - newMonstersAdded);
				}
				else
					currentNewMonsterDelay--;

			}

			dungeonManager.ForceRedraw();
		}

		public void NewLevel()
		{
			currentNewMonsterDelay = newMonsterDelay - dungeonManager.CurrentDungeonLevel;
			newMonstersAdded = 0;
		}

		public void CreaturesToTiles()
		{

			Monsters.ToList().ForEach(m => dungeonManager.DungeonLevel.AddTile(m, Layers.Creature));
			dungeonManager.DungeonLevel.AddTile(player, Layers.Creature);

		}

		/// <summary>
		/// Loop through monsters and move them toward player position.
		/// </summary>
		private void MoveMonsters()
		{
			foreach (Monster m in Monsters)
				m.Flags["hasMoved"] = false;

			int loops = 0;
			int maxLoops = Monsters.ToList().Where(m => !m.Flags["hasMoved"]).Count() + 100;

			while (Monsters.ToList().Where(m => !m.Flags["hasMoved"]).Count() != 0 && loops < maxLoops)
			{

				//prevents infinite loop in release only.
#if DEBUG
#else
				loops++;
#endif
				Monsters.ToList().ForEach(m =>
					{
						if (!m.Flags["hasMoved"])
							m.MoveToPlayer(player.Position, Monsters);
					});
			}

		}

		/// <summary>
		/// Finds if a monster and player are on the same square then calls Fight
		/// This should make it easier to test.  test 1 collision test 2 fight.
		/// </summary>
		public void CreatureCollisions()
		{

			//List all monsters in battle, as monsters can't stack this should always be one.
			List<Monster> monstersInBattle = Monsters.Where(m => m.X == player.X && m.Y == player.Y).ToList();

			//call fight
			Fight(monstersInBattle);
			
			//Changed to descending so we don't miss any traps. 
			for (int i = DungeonManager.Instance.PropManager.
				 Props.OfType<Trap>().Where(t => t.IsSet == true).Count(); i-- > 0;)
			{
				Trap trap =
					DungeonManager.Instance.PropManager.
					Props.OfType<Trap>().Where(t => t.IsSet == true).ToArray()[i];
				Monster monster = Monsters.Where(m => m.X == trap.X && m.Y == trap.Y).FirstOrDefault();
				if (monster != null)
				{
					VisualizeBattle(monster);
					trap.Kill(monster);
					dungeonManager.PropManager.Remove(trap);
					dungeonManager.ForceRedraw();
				}
			}

		}

		/// <summary>
		/// Fight Monsters On PlayersTile
		/// </summary>
		/// <returns>string</returns>
		/// 
		private bool Fight(List<Monster> monstersInBattle)
		{
			bool fought = false;
			if (monstersInBattle.Count() > 0)
			{
				SimpleBattle.Fight(player, monstersInBattle);

				VisualizeBattle(player);
				fought = true;
			}

			return fought;
		}

		private void VisualizeBattle(Creature creature)
		{
			//should have a special battle tile.
			string oldCharacter = creature.ImageCharacter;
			ConsoleColor oldColor = creature.Color;

			creature.ImageCharacter = "*";

			dungeonManager.PlaySound(SoundList.Battle);

			dungeonManager.DungeonVisualizer.Flash(creature, 10, Properties.Settings.Default.AnimationSpeed);

			creature.ImageCharacter = oldCharacter;
			creature.Color = oldColor;

			dungeonManager.ForceRedraw();
		}

		public bool PlayerAlive
		{
			get { return playerAlive; }
		}

		public Player Player
		{
			get { return player; }
		}

		#endregion

		#region events
		void monster_CreatureDeadEvent(object sender,RaiseStatArgs e)
		{
			Monsters.Remove((Monster)sender);
		}

		private void player_CreatureDeadEvent(object sender, RaiseStatArgs e)
		{
			playerAlive = false;
		}
		#endregion

	}
}
