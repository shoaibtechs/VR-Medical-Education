namespace MAGES.Networking
{
#if PHOTON_UNITY_NETWORKING

    using MAGES.SceneGraph;
    using Photon.Pun;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Syncs the state of the question between users.
    /// </summary>
    public class SyncQuestionState : MonoBehaviourPunCallbacks
    {
        private QuestionBehavior questionBehaviour;

        private void Start()
        {
            // Add functions to buttons
            questionBehaviour = GetComponent<QuestionBehavior>();

            if (questionBehaviour == null)
            {
                Debug.LogError("SyncQuestionState: QuestionBehaviour not found in object " + gameObject.name + ".");
                return;
            }

            // Setup the answer network callbacks
            foreach (QuestionActionAnswer answer in questionBehaviour.Answers)
            {
                answer.AnswerObject.GetComponent<Toggle>().onValueChanged.AddListener((value) =>
                {
                    // Get the index of the answer
                    int index = questionBehaviour.Answers.IndexOf(answer);

                    // Convert index to byte
                    byte byteIndex = (byte)index;

                    SendSelected(byteIndex, value);
                });
            }

            // Setup the submit network callbacks
            questionBehaviour.SubmitElement.GetComponent<Button>().onClick.AddListener(() =>
            {
                SendSubmit();
            });
        }

        /// <summary>
        /// Syncs the question scramble between the users.
        /// </summary>
        /// <param name="newOrder">The new order of the answers.</param>
        [PunRPC]
        public void SyncScramble(byte[] newOrder)
        {
            foreach (var answer in questionBehaviour.Answers)
            {
                answer.Index = newOrder[questionBehaviour.Answers.IndexOf(answer)];
            }
        }

        /// <summary>
        /// Syncs the question scramble between the users.
        /// </summary>
        /// <param name="newOrder">The new order of the answers.</param>
        public void SendScrambledOrder(byte[] newOrder)
        {
            photonView.RPC("SyncSelected", RpcTarget.Others, newOrder);
        }

        //--------------------Answer Pressed--------------------

        /// <summary>
        /// Called on other users when a user selects a ui.
        /// </summary>
        /// <param name="index">The index of the selected answer.</param>
        /// <param name="isSelected">Value to indicate if the answer is correct or wrong.</param>
        [PunRPC]
        public void SyncSelected(byte index, bool isSelected)
        {
            // Get the answer object from the index
            QuestionActionAnswer answer = questionBehaviour.Answers[index];

            // Update current button state to the one received from other user.
            questionBehaviour.AnswerPressed(answer.AnswerObject);
        }

        /// <summary>
        /// Broadcasts the selected answer to all other users.
        /// </summary>
        /// <param name="index">The index of the selected answer.</param>
        /// <param name="isSelected">Value to indicate if the answer is correct or wrong.</param>
        public void SendSelected(byte index, bool isSelected)
        {
            photonView.RPC("SyncSelected", RpcTarget.Others, index, isSelected);
        }

        //--------------------Submit Pressed--------------------

        /// <summary>
        /// Sync the submit button between users.
        /// </summary>
        [PunRPC]
        public void SyncSubmit()
        {
            questionBehaviour.SubmitPressed();
        }

        /// <summary>
        /// Sync the submit button between users.
        /// </summary>
        public void SendSubmit()
        {
            photonView.RPC("SyncSubmit", RpcTarget.Others);
        }
    }
#endif
}