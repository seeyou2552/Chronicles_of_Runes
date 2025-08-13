using _02_Scripts.Inventory;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ScoreBoardUI : MonoBehaviour
{
    [Header("Score")]
    [SerializeField] private TextMeshProUGUI totalDamageText;
    [SerializeField] private TextMeshProUGUI totalDamageTakenText;
    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private TextMeshProUGUI runGetCountText;
    [SerializeField] private TextMeshProUGUI magicGetCountText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI totalText;

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI totalDamageScore;
    [SerializeField] private TextMeshProUGUI totalDamageTakenScore;
    [SerializeField] private TextMeshProUGUI killCountScore;
    [SerializeField] private TextMeshProUGUI runGetCountScore;
    [SerializeField] private TextMeshProUGUI magicGetCountScore;
    [SerializeField] private TextMeshProUGUI goldScore;
    [SerializeField] private TextMeshProUGUI totalScore;

    private bool isTyping = false;
    private bool skipTyping = false;

    public IEnumerator OutputScore()
    {
        // totalDamageScore 출력
        totalDamageText.gameObject.SetActive(true);
        yield return StartCoroutine(ScoreDirector(totalDamageScore, GameManager.Instance.totalDamage));

        // totalDamageTakenScore 출력
        totalDamageTakenText.gameObject.SetActive(true);
        yield return StartCoroutine(ScoreDirector(totalDamageTakenScore, GameManager.Instance.totalDamageTaken));

        // killCountScore 출력
        killCountText.gameObject.SetActive(true);
        yield return StartCoroutine(ScoreDirector(killCountScore, GameManager.Instance.killCount));

        // runGetCountScore 출력
        runGetCountText.gameObject.SetActive(true);
        yield return StartCoroutine(ScoreDirector(runGetCountScore, GameManager.Instance.runeGetCount));

        // magicGetCountScore 출력
        magicGetCountText.gameObject.SetActive(true);
        yield return StartCoroutine(ScoreDirector(magicGetCountScore, GameManager.Instance.magicGetCount));

        // GoldScore 출력
        goldText.gameObject.SetActive(true);
        yield return StartCoroutine(ScoreDirector(goldScore, InventoryManager.Instance.Gold));

        // totalScore 출력
        int tempScore = GameManager.Instance.ScoreCalculate();
        totalText.gameObject.SetActive(true);
        yield return StartCoroutine(ScoreDirector(totalScore, tempScore ));

        if (tempScore > GameManager.Instance.highScore)
        {
            GameManager.Instance.NewHighScore(tempScore);
            // 이후 뭔가 연출
        }

    }

    void Update()
    {
        if (Input.anyKeyDown && isTyping) // 타이핑 중인 경우 스킵
        {
            skipTyping = true;
            return;
        }
    }

    private IEnumerator ScoreDirector(TextMeshProUGUI scoreText, int tempScore) // 점수 연출
    {
        skipTyping = false;
        isTyping = true;
        int score = 0;

        while (true) // 점수 타이핑
        {
            if (skipTyping) break;
            if (score >= tempScore) break;
            score += 1;
            scoreText.text = score.ToString();
            yield return new WaitForSeconds(0.01f);
        }

        scoreText.text = tempScore.ToString();
        isTyping = false;
    }

    public void ClearScore()
    {
        totalDamageScore.text = "";
        totalDamageText.gameObject.SetActive(false);

        totalDamageTakenScore.text = "";
        totalDamageTakenText.gameObject.SetActive(false);

        killCountScore.text = "";
        killCountText.gameObject.SetActive(false);

        runGetCountScore.text = "";
        runGetCountText.gameObject.SetActive(false);

        magicGetCountScore.text = "";
        magicGetCountText.gameObject.SetActive(false);

        goldScore.text = "";
        goldText.gameObject.SetActive(false);

        totalScore.text = "";
        totalText.gameObject.SetActive(false);
    }

}
