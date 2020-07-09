using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCanvasController : MonoBehaviour
{
    public Text combatText;
    public Text healthText;
    public Text turnOrderIndicator;
    public Transform combatTextTarget;
    Vector3 combatTextInitialPos, combatTextTargetPos;
    public float textScrollDistancePerSec;
    float t;
    public Coroutine m_combatTextScroll;
    public WaitUntil sceneLoadFinished;
    public Image healthbar;
    public GameObject statusPassive, statusProximity, statusAggressive, actionPoint;
    public GameObject actionPointBar;
    public List<GameObject> actionPoints = new List<GameObject>();

    void Start()
    {
        combatText.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        transform.rotation = Camera.main.transform.rotation;
    }

    public void SetActionPointsToMax(CharacterScript character)
    {    
        if(character.behaviour == CharacterBehaviourType.passive)
            return;

        for(int j = 0; j < actionPoints.Count; ++j)
        {
            Destroy(actionPoints[0]);
            actionPoints.RemoveAt(0);
            --j;
        }

        for(int i = 0; i < character.maxActionPoints; ++i)
        {
            var ap = Instantiate(actionPoint, actionPointBar.transform.position, actionPointBar.transform.rotation, actionPointBar.transform);
            actionPoints.Add(ap);
        }
    }

    public void RemoveActionPoints(int count)
    {
        for(int i = 0; i < count; ++i)
        {
            Destroy(actionPoints[0]);
            actionPoints.RemoveAt(0);
        }
    }

    public void AddActionPoints(int count)
    {
        for(int i = 0; i < count; ++i)
        {
            var ap = Instantiate(actionPoint, actionPointBar.transform.position, actionPointBar.transform.rotation, actionPointBar.transform);
            actionPoints.Add(ap);
        }
    }
    
    public void SetHealthbarFill(int maxHealth, int currentHealth)
    {
        if(currentHealth <= 0 || maxHealth <= 0)
        {
            healthbar.fillAmount = 0;
            healthText.text = "0";
            
            return;
        }
        else
        {
            healthbar.fillAmount = (float)((float)currentHealth / (float)maxHealth);
            healthText.text = currentHealth.ToString();
        }
    }

    IEnumerator AttackScroll(int maxHealth, int currentHealth, bool healthbarFillSet = false)
    {
        yield return null;

        if(!healthbarFillSet)
        {
            SetHealthbarFill(maxHealth, currentHealth);
            healthbarFillSet = true;
        }
            
        t = Mathf.Clamp01(t + Time.deltaTime * textScrollDistancePerSec);

        combatText.transform.position = Vector3.Lerp(combatTextInitialPos, combatTextTargetPos, t);

        if(t == 1)
        {
            combatText.transform.position =  combatTextInitialPos;
            combatText.gameObject.SetActive(false);
            StopCoroutine(m_combatTextScroll);
            m_combatTextScroll = null;
        }
        else
            m_combatTextScroll = StartCoroutine(AttackScroll(maxHealth, currentHealth, healthbarFillSet));
    }

    public void Attacked(int maxHealth, int currentHealth, int damage)
    {
        if(m_combatTextScroll != null)
        {
            combatText.transform.position = combatTextInitialPos;
            StopCoroutine(m_combatTextScroll);
            m_combatTextScroll = null;
        }

        t = 0;
        combatTextInitialPos = combatText.transform.position;
        combatTextTargetPos = combatTextTarget.position;
        combatText.gameObject.SetActive(true);
        combatText.text = damage.ToString();
        m_combatTextScroll = StartCoroutine(AttackScroll(maxHealth, currentHealth));
    }
}
