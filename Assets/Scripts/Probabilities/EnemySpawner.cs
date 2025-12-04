using UnityEngine ;

[System.Serializable]
public class Enemy {
   // for debug :
   public string Name ;

   public GameObject Prefab ;
   [Range (0f, 100f)]public float Chance = 100f ;

   [HideInInspector] public double _weight ;
}


public class EnemySpawner : MonoBehaviour {
   [SerializeField] private Enemy[] enemies ;

   private double accumulatedWeights ;
   public int EnemyTotal = 10;
   private System.Random rand = new System.Random () ;


   private void Awake () {
      CalculateWeights () ;
   }


   private void Start () {
      for (int i = 0; i < EnemyTotal; i++)
         SpawnRandomEnemy (new Vector2 (Random.Range (-10f, 10f), Random.Range (-8f, 8f))) ;
   }

   private void SpawnRandomEnemy (Vector2 position) {
      Enemy randomEnemy = enemies [ GetRandomEnemyIndex () ] ;

      Instantiate (randomEnemy.Prefab, position, Quaternion.identity, transform) ;

      // This line is not required (debug) :
      Debug.Log ("<color=" + randomEnemy.Name + ">●</color> Chance: <b>" + randomEnemy.Chance + "</b>%") ;
   }

   private int GetRandomEnemyIndex () {
      double r = rand.NextDouble () * accumulatedWeights ;

      for (int i = 0; i < EnemyTotal; i++)
         if (enemies [ i ]._weight >= r)
            return i ;

      return 0 ;
   }

   private void CalculateWeights () {
      accumulatedWeights = 0f ;
      foreach (Enemy enemy in enemies) {
         accumulatedWeights += enemy.Chance ;
         enemy._weight = accumulatedWeights ;
      }
   }
}