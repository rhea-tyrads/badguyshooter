using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class TutorialController : MonoBehaviour
    {
        static TutorialController tutorialController;
        static List<ITutorial> registeredTutorials = new();

        [SerializeField] TutorialCanvasController tutorialCanvasController;
        [SerializeField] NavigationArrowController navigationArrowController;

        [Space]
        [SerializeField] GameObject labelPrefab;

        [Space]
        [SerializeField] bool activateWeaponTutorial = true;
        [SerializeField] bool activateCharacterTutorial = true;

        static Pool labelPool;

        static bool isTutorialSkipped;

        public static bool ActivateWeaponTutorial => tutorialController.activateWeaponTutorial;
        public static bool ActivateCharacterTutorial => tutorialController.activateCharacterTutorial;

        public void Initialise()
        {
            tutorialController = this;
            isTutorialSkipped = TutorialHelper.IsTutorialSkipped();
            labelPool = new Pool(new PoolSettings(labelPrefab.name, labelPrefab, 0, true));
            navigationArrowController.Initialise();
            tutorialCanvasController.Initialise();
        }

        void LateUpdate()
        {
            navigationArrowController.LateUpdate();
        }

        public static ITutorial GetTutorial(TutorialID tutorialID)
        {
            foreach (var tutorial in registeredTutorials.Where(tutorial => tutorial.TutorialID == tutorialID))
            {
                if (!tutorial.IsInitialised) tutorial.Initialise();
                if (isTutorialSkipped) tutorial.FinishTutorial();

                return tutorial;
            }

            return null;
        }

        public static void ActivateTutorial(ITutorial tutorial)
        {
            if (!tutorial.IsInitialised) tutorial.Initialise();
            if (isTutorialSkipped) tutorial.FinishTutorial();
        }

        public static void RegisterTutorial(ITutorial tutorial)
        {
            if (registeredTutorials.FindIndex(x => x == tutorial) != -1)
                return;

            registeredTutorials.Add(tutorial);
        }

        public static void RemoveTutorial(ITutorial tutorial)
        {
            var tutorialIndex = registeredTutorials.FindIndex(x => x == tutorial);
            if (tutorialIndex != -1)
                registeredTutorials.RemoveAt(tutorialIndex);
        }

        public static TutorialLabelBehaviour CreateTutorialLabel(string text, Transform parentTransform, Vector3 offset)
        {
            var labelObject = labelPool.Get();
            labelObject.transform.position = parentTransform.position + offset;

            var tutorialLabelBehaviour = labelObject.GetComponent<TutorialLabelBehaviour>();
            tutorialLabelBehaviour.Activate(text, parentTransform, offset);

            return tutorialLabelBehaviour;
        }

        public static void Unload()
        {
            labelPool.ReturnToPoolEverything(true);
            tutorialController.navigationArrowController.Unload();
        }
    }
}