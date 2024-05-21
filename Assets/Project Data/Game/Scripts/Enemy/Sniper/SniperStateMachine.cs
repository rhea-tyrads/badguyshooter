using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Sniper
{
    [RequireComponent(typeof(SniperEnemyBehavior))]
    public class SniperStateMachine : AbstractStateMachine<State>
    {
        SniperEnemyBehavior enemy;

        void Awake()
        {
            enemy = GetComponent<SniperEnemyBehavior>();

            var patrollingStateCase = new StateCase();
            patrollingStateCase.state = new PatrollingState(enemy);
            patrollingStateCase.transitions = new List<StateTransition<State>> {
                new(PatrollingStateTransition)
            };

            var followingStateCase = new StateCase();
            followingStateCase.state = new FollowingState(enemy);
            followingStateCase.transitions = new List<StateTransition<State>>
            {
                new(FollowingStateTransition)
            };

            var fleeingStateCase = new StateCase();
            fleeingStateCase.state = new FleeingState(enemy);
            fleeingStateCase.transitions = new List<StateTransition<State>>
            {
                new(FleeingStateTransition)
            };

            var attackingStateCase = new StateCase();
            attackingStateCase.state = new AimAndAttackState(enemy);
            attackingStateCase.transitions = new List<StateTransition<State>>()
            {
                new(AttackingStateTransition)
            };

            var aimCase = new StateCase();
            aimCase.state = new SniperAimState(enemy);
            aimCase.transitions = new List<StateTransition<State>>()
            {
                new(AimTransition, StateTransitionType.OnFinish)
            };

            states.Add(State.Patrolling, patrollingStateCase);
            states.Add(State.Following, followingStateCase);
            states.Add(State.Fleeing, fleeingStateCase);
            states.Add(State.Attacking, attackingStateCase);
            states.Add(State.Aiming, aimCase);
        }

        bool PatrollingStateTransition(out State nextState)
        {
            var isTargetSpotted = enemy.IsTargetInVisionRange || (!EnemyController.IgnoreAttackAfterDamage && enemy.HasTakenDamage);

            if (!isTargetSpotted)
            {
                nextState = State.Patrolling;
                return false;
            }

            if (enemy.IsTargetInFleeRange)
                nextState = State.Fleeing;
            else if (enemy.IsTargetInAttackRange)
                nextState = State.Aiming;
            else
                nextState = State.Following;
            return true;
        }

        bool FollowingStateTransition(out State nextState)
        {
            if (enemy.IsTargetInFleeRange)
            {
                nextState = State.Fleeing;
                return true;
            }
            else if (enemy.IsTargetInAttackRange && enemy.IsTargetInSight())
            {
                nextState = State.Aiming;
                return true;
            }

            nextState = State.Following;
            return false;
        }

        bool FleeingStateTransition(out State nextState)
        {
            if (enemy.IsTargetInAttackRange)
            {
                nextState = State.Fleeing;
                return false;
            }

            nextState = State.Following;

            return true;
        }

        bool AttackingStateTransition(out State nextState)
        {
            var attackingState = states[State.Attacking].state;
            if ((attackingState as AimAndAttackState).IsFinished && !CharacterBehaviour.IsDead)
            {
                if (enemy.IsTargetInFleeRange)
                    nextState = State.Fleeing;
                else if (enemy.IsTargetInAttackRange)
                    nextState = State.Aiming;
                else
                    nextState = State.Following;

                return true;
            }

            nextState = State.Attacking;
            return false;
        }

        bool AimTransition(out State nextState)
        {
            nextState = State.Attacking;
            return true;
        }
    }
}