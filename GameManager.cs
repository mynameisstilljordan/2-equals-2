using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
using Lofelt.NiceVibrations;
using GleyMobileAds;

public class GameManager : MonoBehaviour{
    //the status for the numbers
    enum Status{
        Idle,
        Moving,
        Winning,
        Failing
    };

    [SerializeField] private Status _currentStatus;
    [SerializeField] private Canvas _helpCanvas;
    [SerializeField] private GameObject _buttonHolder;
    [SerializeField] private GameObject[] _equationPlaceholders;
    [SerializeField] private TMP_Text _num1Text, _num2Text, _num3Text, _num4Text, _operator1Text, _operator2Text, _levelText, _LHSText, _RHSText, _solutionRevealedText;
    [SerializeField] Button[] _allButtons;
    [SerializeField] Button _homeButton, _helpButton, _closeButton, _revealSolutionButton;
    
    char[] _allOperators = new char[] { '+', '-', '×', '÷' };
    char _operator1, _operator2; //the 2 operators
    int _baseNumber; //the number that both sides equal in the end
    int _num1, _num2, _num3, _num4; 
    public int _choice1, _choice2, _choice3, _choice4;
    private int _level; //the score int
    private string _numberPlaceholder = "";
    private Sequence _winSequence, _failSequence;
    private bool _isSolutionRevealed;

    // Start is called before the first frame update
    void Start() {
        //if theres no banner, show it
        Advertisements.Instance.ShowBanner(BannerPosition.BOTTOM);
        
        _level = PlayerPrefs.GetInt("level", 1); //load the level playerprefs
        _levelText.text = _level.ToString(); //update the level text
        Random.InitState(_level); //set seed to level
        
        //the win sequence
        _winSequence = DOTween.Sequence().Pause();
        _winSequence.Insert(0,_num1Text.DOColor(Color.green, 0.5f));
        _winSequence.Insert(0,_num1Text.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.3f));
        _winSequence.Insert(0,_num2Text.DOColor(Color.green, 0.5f));
        _winSequence.Insert(0,_num2Text.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.3f));
        _winSequence.Insert(0,_num3Text.DOColor(Color.green, 0.5f));
        _winSequence.Insert(0,_num3Text.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.3f));
        _winSequence.Insert(0,_num4Text.DOColor(Color.green, 0.5f));
        _winSequence.Insert(0,_num4Text.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.3f).OnComplete(() => {
            _num1Text.DOColor(Color.white, 0.5f);
            _num1Text.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f);
            _num2Text.DOColor(Color.white, 0.5f);
            _num2Text.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f);
            _num3Text.DOColor(Color.white, 0.5f);
            _num3Text.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f);
            _num4Text.DOColor(Color.white, 0.5f);
            _num4Text.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f);
        }));
        
        //the fail sequence
        _failSequence = DOTween.Sequence().Pause();
        _failSequence.Insert(0,_num1Text.DOColor(Color.red, 0.5f));
        _failSequence.Insert(0,_num1Text.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.3f));
        _failSequence.Insert(0,_num2Text.DOColor(Color.red, 0.5f));
        _failSequence.Insert(0,_num2Text.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.3f));
        _failSequence.Insert(0,_num3Text.DOColor(Color.red, 0.5f));
        _failSequence.Insert(0,_num3Text.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.3f));
        _failSequence.Insert(0,_num4Text.DOColor(Color.red, 0.5f));
        _failSequence.Insert(0,_num4Text.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.3f).OnComplete(() => {
            _num1Text.DOColor(Color.white, 0.5f);
            _num1Text.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f);
            _num2Text.DOColor(Color.white, 0.5f);
            _num2Text.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f);
            _num3Text.DOColor(Color.white, 0.5f);
            _num3Text.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f);
            _num4Text.DOColor(Color.white, 0.5f);
            _num4Text.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f);
        }));
        _failSequence.AppendCallback(() => { _failSequence.Rewind();}); //rewind the sequence one it has been completed
        
        DOTween.Init();
        
        _currentStatus = Status.Idle; //initialize the status

        GenerateEquation(); //generate the equation
        
        //set text of buttons
        _allButtons[0].transform.GetChild(0).GetComponent<TMP_Text>().text = _num1.ToString();
        _allButtons[1].transform.GetChild(0).GetComponent<TMP_Text>().text = _num2.ToString();
        _allButtons[2].transform.GetChild(0).GetComponent<TMP_Text>().text = _num3.ToString();
        _allButtons[3].transform.GetChild(0).GetComponent<TMP_Text>().text = _num4.ToString();
        
        //add listener to number buttons
        _allButtons[0].onClick.AddListener(() => { OnButtonPressed(_allButtons[0], _num1); }); 
        _allButtons[1].onClick.AddListener(() => { OnButtonPressed(_allButtons[1], _num2); }); 
        _allButtons[2].onClick.AddListener(() => { OnButtonPressed(_allButtons[2], _num3); }); 
        _allButtons[3].onClick.AddListener(() => { OnButtonPressed(_allButtons[3], _num4); });

        //update the operator text
        _operator1Text.text = _operator1.ToString();
        _operator2Text.text = _operator2.ToString();
        
        //mix the buttons
        MixButtons();
        
        //home button function
        _homeButton.onClick.AddListener(OnHomeButtonPressed);
        
        //help button function
        _helpButton.onClick.AddListener(OnHelpButtonPressed);
        
        //close button function
        _closeButton.onClick.AddListener(OnCloseButtonPressed);

        //reveal button function
        _revealSolutionButton.onClick.AddListener(OnSolutionRevealButtonPressed);

        //if the user is not connected to the internet
        if (Application.internetReachability == NetworkReachability.NotReachable) {
            _revealSolutionButton.interactable = false; //disable the watch ad button
        }
        
        //set the isSolutionRevealed bool depending on the playerpref
        if (PlayerPrefs.GetInt("isSolutionRevealed", 0) == 1) _isSolutionRevealed = true;

        //if the solution is revealed
        if (_isSolutionRevealed) {
            _revealSolutionButton.interactable = false; //disable the button
            _solutionRevealedText.text = _num1 + " " + _operator1 + " " + _num2 + " = " + _num3 + " " + _operator2 + " " + _num4; //set the text to the solution
            _solutionRevealedText.color = Color.white; //set the text color to white
        }
    }

    //this method generates the entire equation
    void GenerateEquation() {
        GenerateBaseNumber(); //generate the base number
        GenerateOperators(); //generate the operators
        GenerateLHSNumbers(); //generate the LHS numbers
        GenerateRHSNumbers(); //generate the RHS numbers
    }

    #region EquationGeneration
    //this method gives the base number a value
    void GenerateBaseNumber(){
        _baseNumber = Random.Range(0, 50);
    }

    //this method generates the operators
    void GenerateOperators() {
        //if level is 50+, operators cannot be the same
        if (_level > 50) {
            while (_operator1 == _operator2) {
                _operator1 = _allOperators[Random.Range(0, 4)]; //choose the first operator
                _operator2 = _allOperators[Random.Range(0, 4)]; //choose the second operator
            }
        }

        //if level is greater than 35, introduce division
        else if (_level > 35){
            _operator1 = _allOperators[Random.Range(0, 4)];
            _operator2 = _allOperators[Random.Range(0, 4)];
        }
        
        //if level is greater than 25, introduce multiplication
        else if (_level > 25) {
            _operator1 = _allOperators[Random.Range(0, 3)];
            _operator2 = _allOperators[Random.Range(0, 3)];
        }
        
        //if level is greater than 10, introduce subtraction
        else if (_level > 10) {
            _operator1 = _allOperators[Random.Range(0, 2)];
            _operator2 = _allOperators[Random.Range(0, 2)];
        }

        //if level is between 1 and 10, only use addition
        else {
            _operator1 = _allOperators[0];
            _operator2 = _allOperators[0];
        }
    }

    void GenerateLHSNumbers() {
        int rhsValue = 0; //the rhs value of the LHS of the equation.  For example for 8 + 2, 2 is the rhs value
        int resultantValue = 0; //the resultant value after the rhs value is applied.  For example, with a base number of 10 and a rhs value of 4 (in subtraction), 4 + 10 = 14.  14 is the resultantValue [addition here because we use the inverse to create the equation]

        switch (_operator1) {
            case '+':
                rhsValue = Random.Range(0, 21);
                resultantValue = _baseNumber - rhsValue;
                break;
            case '-':
                rhsValue = Random.Range(0, 21);
                resultantValue = _baseNumber + rhsValue;
                break;
            case '×':
                rhsValue = Random.Range(1, 6); //initialization for equation checking
                //while the rhs value is not a perfect denominator
                while ((_baseNumber%rhsValue) != 0){
                    rhsValue = Random.Range(1, 6); //set the rhs to a random value
                }
                resultantValue = _baseNumber / rhsValue;
                break;
            case '÷':
                rhsValue = Random.Range(1, 6);
                resultantValue = _baseNumber * rhsValue;
                break;
        }

        //save num1 and num2
        _num1 = resultantValue;
        _num2 = rhsValue;
    }

    void GenerateRHSNumbers() {
        int rhsValue = 0; //the rhs value of the LHS of the equation.  For example for 8 + 2, 2 is the rhs value
        int resultantValue = 0; //the resultant value after the rhs value is applied.  For example, with a base number of 10 and a rhs value of 4 (in subtraction), 4 + 10 = 14.  14 is the resultantValue [addition here because we use the inverse to create the equation]

        switch (_operator2) {
            case '+':
                rhsValue = Random.Range(0, 21);
                resultantValue = _baseNumber - rhsValue;
                break;
            case '-':
                rhsValue = Random.Range(0, 21);
                resultantValue = _baseNumber + rhsValue;
                break;
            case '×':
                rhsValue = Random.Range(1, 6); //initialization for equation checking
                //while the rhs value is not a perfect denominator
                while ((_baseNumber % rhsValue) != 0) {
                    rhsValue = Random.Range(1, 6); //set the rhs to a random value
                }
                resultantValue = _baseNumber / rhsValue;
                break;
            case '÷':
                rhsValue = Random.Range(1, 6);
                resultantValue = _baseNumber * rhsValue;
                break;
        }

        //save num1 and num2
        _num3 = resultantValue;
        _num4 = rhsValue;
    }
    #endregion
    #region EqualityCheck

    //check the equality with operator 1
    bool CheckEqualityWithOperator1 (int num1, int num2) {
        switch (_operator1) {
            case '+':
                if ((num1 + num2) == _baseNumber) return true;
                break;
            case '-':
                if ((num1 - num2) == _baseNumber) return true;
                break;
            case '×':
                if ((num1 * num2) == _baseNumber) return true;
                break;
            case '÷':
                if ((num1 / num2) == _baseNumber) return true;
                break;
        }
        return false; //otherwise, return false
    }

    //check the equality with operator 2
    bool CheckEqualityWithOperator2(int num1, int num2) {
        switch (_operator2) {
            case '+':
                if ((num1 + num2) == _baseNumber) return true;
                break;
            case '-':
                if ((num1 - num2) == _baseNumber) return true;
                break;
            case '×':
                if ((num1 * num2) == _baseNumber) return true;
                break;
            case '÷':
                if ((num1 / num2) == _baseNumber) return true;
                break;
        }
        return false; //otherwise, return false
    }

    //this method gets the result of the LHS
    private int GetLHS(int num1, int num2) {
        switch (_operator1) {
            case '+':
                return num1 + num2;
            case '-':
                return num1 - num2;
            case '×':
                return num1 * num2;
            case '÷':
                return num1 / num2;
        }
        return 0;
    }
    
    //this method gets the result of the RHS
    private int GetRHS(int num1, int num2) {
        switch (_operator2) {
            case '+':
                return num1 + num2;
            case '-':
                return num1 - num2;
            case '×':
                return num1 * num2;
            case '÷':
                return num1 / num2;
        }
        return 0;
    }
    bool IsTheEquationCorrect() {
        if  ((IsACorrectEquationFound() || HasAnAlternateSolutionBeenFound()) //if a solution or alternate solution is found
             && !AreThereAnyButtonsLeft()) //if there are no buttons left 
            return true; //if the equation is correct, return true
        
        return false; //otherwise, return false
    }

    //this method checks to see if there is a correct equation found (regardless of the order the numbers were put in)
    bool IsACorrectEquationFound() {
        return (CheckEqualityWithOperator1(_choice1, _choice2) && CheckEqualityWithOperator2(_choice3, _choice4));
    }

    //this method checks if an alternate solution was found
    bool HasAnAlternateSolutionBeenFound() {
        return (GetLHS(_choice1, _choice2) == GetRHS(_choice3, _choice4)); //return true if the LHS and RHS are equal
    }
    #endregion

    //when the button is pressed
    void OnButtonPressed(Button button, int num) {
        
            PlayButtonFeedback(); //play the sound feedback

            //if the button is a child of the button holder
            if (button.transform.parent.CompareTag("ButtonHolder")) {
                bool wasThisTheLastNumberClicked = _buttonHolder.transform.childCount == 1;

                Transform newPosition = null;
                TMP_Text equationText = null;

                if (_num1Text.text == _numberPlaceholder) {
                    _choice1 = num; //set the choice int to num
                    equationText = _num1Text; //get the reference to the TMP_Text
                    newPosition = _equationPlaceholders[0].transform; //set the position to move the button to
                }
                else if (_num2Text.text == _numberPlaceholder) {
                    _choice2 = num; //set the choice int to num
                    equationText = _num2Text; //get the reference to the TMP_Text
                    newPosition = _equationPlaceholders[1].transform; //set the position to move the button to
                }
                else if (_num3Text.text == _numberPlaceholder) {
                    _choice3 = num; //set the choice int to num
                    equationText = _num3Text; //get the reference to the TMP_Text
                    newPosition = _equationPlaceholders[2].transform; //set the position to move the button to
                }
                else if (_num4Text.text == _numberPlaceholder) {
                    _choice4 = num; //set the choice int to num
                    equationText = _num4Text; //get the reference to the TMP_Text
                    newPosition = _equationPlaceholders[3].transform; //set the position to move the button to
                }

                equationText.text = " ";
                
                //_currentStatus = Status.Moving;
                button.transform.SetParent(newPosition); //set the parent to the new position
                button.transform.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                button.transform.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                button.transform.DOLocalMove(Vector2.zero, 0.5f)
                    .SetEase(Ease.OutSine)
                    .OnComplete(() => {
                        button.transform.GetChild(0).GetComponent<TMP_Text>().text = ""; //delete text
                        equationText.text = num.ToString(); //update the text 
                        UpdateTextOnBothSides(); //update the text numbers

                        //if the equation is correct
                        if (IsTheEquationCorrect() && wasThisTheLastNumberClicked) {
                            SoundManager.PlaySound("win"); //play win sound
                            _currentStatus = Status.Winning;
                            _winSequence.Play()
                                .OnComplete(() => {
                                    PlayerPrefs.SetInt("isSolutionRevealed", 0); //reset isSolutionRevealed
                                    PlayerPrefs.SetInt("level", _level + 1); //increase the level playerpref
                                    SceneManager.LoadScene("ingame"); //load the scene again
                                });
                        }

                        //if the equation is incorrect
                        else {
                            if (!AreThereAnyButtonsLeft() && wasThisTheLastNumberClicked) {
                                SoundManager.PlaySound("fail"); //play fail sound
                                _currentStatus = Status.Idle;
                                _failSequence.Play();
                            }
                        }
                    });
            }

            //otherwise, if the button is a part of the equation
            else {
                button.transform.GetComponentInParent<TMP_Text>().text =
                    _numberPlaceholder; //set the text to the placeholder
                button.transform.SetParent(_buttonHolder.transform); //set the parent to button holder
                button.transform.localPosition = Vector2.zero; //reset the position of the button locally
                //update the button text with the current parent text
                button.transform.GetChild(0).GetComponent<TMP_Text>().text = num.ToString();
                UpdateChoices(); //update the choice variables
                UpdateTextOnBothSides(); //updates the values on both sides
            }
    }

    //this method updates all choices depending on the value held by each number holder
    private void UpdateChoices() {
        if (_num1Text.text == _numberPlaceholder) _choice1 = 0;
        if (_num2Text.text == _numberPlaceholder) _choice2 = 0;
        if (_num3Text.text == _numberPlaceholder) _choice3 = 0;
        if (_num4Text.text == _numberPlaceholder) _choice4 = 0;
    }

    //this method checks to see if there's any buttons left
    private bool AreThereAnyButtonsLeft() {
        return (_buttonHolder.transform.childCount != 0); //return false if there's no buttons left 
    }

    //this method mixes the buttons around
    private void MixButtons() {
        //for i iterations
        for (int i = 0; i < 20; i++) {
            _allButtons[Random.Range(0,4)].transform.SetSiblingIndex(0); //pick a random button and move its sibling index to 0
        }
    }

    //when the home button is pressed
    private void OnHomeButtonPressed() {
        PlayButtonFeedback(); //play the sound feedback
        SceneManager.LoadScene("menu");
        if (Advertisements.Instance.IsBannerOnScreen()) Advertisements.Instance.HideBanner(); //hide the banner if active
    }
    
    //when the help button is pressed
    private void OnHelpButtonPressed() {
        PlayButtonFeedback(); //play the sound feedback
        _helpCanvas.enabled = true;
        if (Advertisements.Instance.IsBannerOnScreen()) Advertisements.Instance.HideBanner(); //hide the banner if active
    }

    //when the close button is pressed
    private void OnCloseButtonPressed() {
        PlayButtonFeedback(); //play the sound feedback
        _helpCanvas.enabled = false;
        if (!Advertisements.Instance.IsBannerOnScreen()) Advertisements.Instance.ShowBanner(BannerPosition.BOTTOM);
    }

    //when the solution reveal button is pressed
    private void OnSolutionRevealButtonPressed() {
        PlayButtonFeedback(); //play the sound feedback

        //if a rewarded ad is available, show it
        if (Advertisements.Instance.IsRewardVideoAvailable())
            Advertisements.Instance.ShowRewardedVideo(RevealSolution);
    }

    //this method updates the text totals for both sides
    private void UpdateTextOnBothSides() {
        _LHSText.text = GetLHS(_choice1,_choice2).ToString();
        _RHSText.text = GetRHS(_choice3,_choice4).ToString();
    }

    //reveal the solution
    private void RevealSolution(bool completed) {
        //if the rewarded ad was completed, show the solution
        if (completed) {
            PlayerPrefs.SetInt("isSolutionRevealed", 1);
            _revealSolutionButton.interactable = false; //disable the button
            _solutionRevealedText.text =
                _num1 + " " + _operator1 + " " + _num2 + " = " + _num3 + " " + _operator2 + " " +
                _num4; //set the text to the solution
            _solutionRevealedText.color = Color.white; //set the text color to white
        }
    }
    
    //this method plays the button sound
    void PlayButtonFeedback() {
        SoundManager.PlaySound("click"); //play sound
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact); //play impact
    }
}
