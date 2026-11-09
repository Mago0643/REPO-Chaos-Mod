using Lego.ChaosMod.Backend;
using UnityEngine;

namespace Lego.ChaosMod.Events;

// WHY THE FUCK THIS DOES NOT WORK
// I DON'T EVEN KNOW HOW TO DEBUG THIS
public class PlayMonsterSoundEvent: ChaosEvent
{
    public PlayMonsterSoundEvent(): base("evt_hallucination", "evt_hallucination_desc")
    {
        isOnce = true;
        ID = 7;
    }

    public override void Start()
    {
        base.Start();

        // ((MonsterSound)Instance).options.chance -= 0.05f;
        // ChaosMod.Instance.EnemyAS.PlayOneShot(sounds[Random.Range(1, sounds.Count)], 0.5f);
        AudioClip clip = null!;
        float volume = 1f;
        float pitch = 1f;
        bool getout = false;
        foreach (EnemyParent enemy in GameObject.FindObjectsByType<EnemyParent>(0)) {
            if (Random.Range(0f, 1f) <= 0.5f) { // 50% chance
                // checks if it is right enemy to play a sound
                // hardcode for most enemies
                // TODO: how
                switch (enemy.name) {
                    case "Clown": {
                        var anim = enemy.transform.GetComponentInChildren<EnemyBeamerAnim>(true);
                        clip = anim.soundAttackIntro.Sounds[0];
                        volume = anim.soundAttackIntro.Volume;

                        getout = true;
                        break;
                    }
                    case "Animal": {
                        var anim = enemy.transform.GetComponentInChildren<EnemyAnimalAnim>(true);
                        clip = anim.noticeSound.Sounds[Random.Range(0, anim.noticeSound.Sounds.Length)];
                        volume = 0.3f;

                        getout = true;
                        break;
                    }
                    case "Chef": {
                        getout = true;
                        break;
                    }
                    case "Cleanup Crew": {
                        var anim = enemy.transform.GetComponentInChildren<EnemyBombThrowerAnim>(true);
                        clip = anim.soundTorsoHeadsScream.Sounds[0];
                        volume = 0.45f;
                        getout = true;
                        break;
                    }
                    case "Apex Predator": {
                        var anim = enemy.transform.GetComponentInChildren<EnemyDuckAnim>(true);
                        clip = anim.noticeSound.Sounds[Random.Range(0, anim.noticeSound.Sounds.Length)];
                        volume = 0.4f;
                        getout = true;
                        break;
                    }
                    case "Elsa": {
                        var anim = enemy.transform.GetComponentInChildren<EnemyElsaAnim>(true);
                        clip = anim.barkSmallSound.Sounds[Random.Range(0, anim.barkSmallSound.Sounds.Length)];
                        volume = 0.375f + Random.Range(0f, 0.02f);
                        pitch += Random.Range(0f, 0.1f);
                        getout = true;
                        break;
                    }
                    case "Mentalist": {
                        var anim = enemy.transform.GetComponentInChildren<EnemyFloaterAnim>(true);
                        clip = anim.sfxMoveLong.Sounds[Random.Range(0, anim.sfxMoveLong.Sounds.Length)];
                        volume = 0.3f + Random.Range(0f, 0.05f);
                        pitch += Random.Range(0f, 0.1f);
                        getout = true;
                        break;
                    }
                    case "Gnome": {
                        var anim = enemy.transform.GetComponentInChildren<EnemyGnomeAnim>(true);
                        clip = anim.soundNotice.Sounds[Random.Range(0, anim.soundNotice.Sounds.Length)];
                        volume = 0.2f + Random.Range(0f, 0.05f);
                        pitch += Random.Range(0f, 0.05f);
                        getout = true;
                        break;
                    }
                    case "Headman": {
                        var anim = enemy.transform.GetComponentInChildren<EnemyHeadAnimationSystem>(true);
                        clip = anim.ChaseBegin.Sounds[0]; // i wanted to find that piano sound but theres too many sounds to find one
                        volume = anim.ChaseBegin.Volume;
                        getout = true;
                        break;
                    }
                    case "Headgrab": {
                        var anim = enemy.transform.GetComponentInChildren<EnemyHeadGrabberAnim>(true);
                        clip = anim.soundNotice.Sounds[0];
                        volume = 0.4f;
                        break;
                    }
                    case "Heart Hugger": {
                        var scr = enemy.transform.GetComponentInChildren<EnemyHeartHugger>(true);
                        clip = scr.soundGrow.Sounds[Random.Range(0, scr.soundGrow.Sounds.Length)];
                        volume = 0.5f + Random.Range(0f, 0.1f);
                        pitch += Random.Range(0f, 0.1f);
                        getout = true;
                        break;
                    }
                    case "Hidden": {
                        var anim = enemy.transform.GetComponentInChildren<EnemyHiddenAnim>(true);
                        clip = anim.soundBreatheInFast.Sounds[Random.Range(0, anim.soundBreatheInFast.Sounds.Length)];
                        volume = 0.1f + Random.Range(0f, 0.02f);
                        pitch += Random.Range(0f, 0.1f);
                        getout = true;
                        break;
                    }
                    case "Huntsman": {
                        var anim = enemy.transform.GetComponentInChildren<EnemyHunterAnim>(true);
                        clip = anim.aimStartClips[Random.Range(0, anim.aimStartClips.Length)];
                        volume = 0.5f;
                        getout = true;
                        break;
                    }
                    case "Trudge": {
                        var anim = enemy.transform.GetComponentInChildren<EnemySlowWalkerAnim>(true);
                        clip = anim.sfxNoticeVoice.Sounds[Random.Range(0, anim.sfxNoticeVoice.Sounds.Length)];
                        volume = 0.5f + Random.Range(0f, 0.1f);
                        pitch += Random.Range(0f, 0.1f);
                        getout = true;
                        break;
                    }
                    case "Tick": {
                        var anim = enemy.transform.GetComponentInChildren<EnemyTickAnim>(true);
                        clip = anim.noticeSound.Sounds[Random.Range(0, anim.noticeSound.Sounds.Length)];
                        volume = 0.5f + Random.Range(0f, 0.1f);
                        pitch += Random.Range(0f, 0.1f);
                        getout = true;
                        break;
                    }
                    case "Bella": {
                        var scr = enemy.transform.GetComponentInChildren<EnemyTricycle>(true);
                        clip = scr.audioBell.Sounds[Random.Range(0, scr.audioBell.Sounds.Length)];
                        volume = 0.8f;
                        getout = true;
                        break;
                    }
                    case "Rugrat": {
                        var anim = enemy.transform.GetComponentInChildren<EnemyValuableThrowerAnim>(true);
                        clip = anim.noticeSound.Sounds[0];
                        volume = 0.4f + Random.Range(0f, 0.05f);
                        pitch += Random.Range(0f, 0.1f);
                        getout = true;
                        break;
                    }
                }
            }

            if (getout) break;
        }

        if (clip != null) {
            ChaosMod.Instance.EnemyAS!.pitch = pitch;
            ChaosMod.Instance.EnemyAS.PlayOneShot(clip, volume);
        }
    }
}