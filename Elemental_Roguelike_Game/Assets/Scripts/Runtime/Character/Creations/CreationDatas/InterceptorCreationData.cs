using UnityEngine;

namespace Runtime.Character.Creations.CreationDatas
{
    [CreateAssetMenu(menuName = "Creation/Interceptor Creation")]
    public class InterceptorCreationData: CreationData
    {

        [SerializeField] private int interceptionScoreMin;
        [SerializeField] private int interceptionScoreMax;

        public int GetInterceptionScore()
        {
            return Random.Range(interceptionScoreMin, interceptionScoreMax);
        }


    }
}