/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Abstract Follower */

using UnityEngine;


namespace PathwaysEngine.Utilities {


    abstract public class AbstractTargetFollower : MonoBehaviour {
        public enum UpdateType { Auto, FixedUpdate, LateUpdate }
        public UpdateType updateType;
        public bool autoTargetPlayer = true;
        [SerializeField] protected Transform target;

        public Transform Target { get { return this.target; } }

        virtual protected void Start() { // inheritors call base.Start()
            if (autoTargetPlayer) FindAndTargetPlayer();
        }

        void FixedUpdate() {
            if (autoTargetPlayer && (!target || !target.gameObject.activeSelf))
                FindAndTargetPlayer();
            if (updateType==UpdateType.FixedUpdate || updateType==UpdateType.Auto
            && target!=null && (target.GetComponent<Rigidbody>()!=null
            && !target.GetComponent<Rigidbody>().isKinematic))
                FollowTarget(Time.deltaTime);
        }

        void LateUpdate() {
            if (autoTargetPlayer && (!target || !target.gameObject.activeSelf))
                FindAndTargetPlayer();
            if (updateType==UpdateType.LateUpdate || updateType==UpdateType.Auto
            && !target && (target.GetComponent<Rigidbody>()==null
            || target.GetComponent<Rigidbody>().isKinematic))
                FollowTarget(Time.deltaTime);
        }

        protected abstract void FollowTarget(float deltaTime);

        public void FindAndTargetPlayer() {
            if (!target) {
//              var targetObj = GameObject.FindGameObjectWithTag("Player");
                var targetObj = Pathways.player.gameObject;
                if (targetObj) SetTarget(targetObj.transform);
            }
        }

        public virtual void SetTarget(Transform t) { target = t; }

    }
}