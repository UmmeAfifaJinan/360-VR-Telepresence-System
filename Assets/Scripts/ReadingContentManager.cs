using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReadingContentManager : MonoBehaviour
{
    public TMP_Text script;
    public Scenario scriptOption;
    public ScrollRect autoScroll;
    public float scrollingSpeed =0.01f;

    public enum Scenario { script1,script2 };
    private string script_1, script_2;
    private float initialYpos;
    private float yPos;

    // Start is called before the first frame update
    void Start()
    {
        script_1 = "Health worker: Good morning. I hope everything is going well with you and your parents. \n 1.Boy: Thank you. Everyone is all right except that Mother has some back trouble.\n 2.Boy: Well, I usually do fine at school, but you know the last year is always difficult.\n 3.Boy: Actually, I've been feeling a bit weak and get these headaches. I thought it was probably malaria, but I am not sure.\n 4.Boy: I've taken the full course of chloroquine tablets about three times so far, but I never seem to get completely well.\n 5.Boy: I think so.\n 6.Boy: My mother always tells us to have a good breakfast, so I make big bowls of cereal for myself and my brothers. Then, too, I always try to buy fruit.\n7.Boy: These jobs are necessary. A few years ago my mother hurt her back. Now it is giving her a lot of trouble. The doctor says she is getting older and there is not much more that can be done. They give her pain-relievers, but the doctor told all of us children to try to help our mother in any way possible. Since I am the oldest, most of the responsibility falls on me.\n8.Boy: I help prepare the evening meal too. I get the smaller children to clean the house, but I have to watch them to see that they do it well.\n 9.Boy: That is a problem. It is really hard to do any serious studying until the chores are done and the younger children have settled down for the night. Then I read for as many hours as possible, or until I just fall asleep at the table.\n10.Boy: As you know, we only have two rooms to live in. One is my parents' bedroom. The other is used for sitting and eating in, and as the children's bedroom. That's why I can't concentrate on my studies until the younger ones are asleep. I even try not to turn the lamp up too bright so they won't wake and disturb me.\n 11.Boy: I guess I never thought about it like that before, but it does make sense. I am worried, however. As you said, I do have to do my chores at home. How can I deal with this problem?\n 12.Boy: I want to pass my exams this year, so I probably need to study more.\n 13.Boy:That's true, so I also have to figure out how to get more rest.\n 14.Boy:Usually after school I walk to the market to pick up the few things I may need for the evening meal. There I meet some friends and we talk and play games for a while. Then when I see the sun is going down, I go home to start the meal.\n 15.Boy:That makes sense. I really like playing with my friends, though.\n 16.Boy:I never thought of it that way, but you are right, I do value my studies and, if I am not in good health, I cannot do well in school. I am sure I could stay after school an extra hour and read at my desk there. No one would disturb me then, and even the teachers might still be around. They could help me with any questions I had. My friends would not miss me for only one hour, so I could join them later. I hope they will not make fun of me for wanting to remain at school.\n 17.Boy:Of course, they always stop by the house at the weekend to say \"hello\" to my mother and ask how she is. I guess they would understand and not make fun of me.\n 18.Boy:Saturday morning is usually taken up with chores. And after that the house is never quiet. The younger children are always running in and out and then there are visitors.\n 19.Boy:Maybe I could see if some of the classrooms at the school are open, or I could even go out to my father's farm. It is always quiet there. I could take some snacks and sit under those big shady trees.\n 20.Boy:The next younger is thirteen, and then there are the twins aged nine.\n 21.Boy:He tries very hard. His grades have been almost as good as mine. He could probably do better.\n 22.Boy:About fourteen.\n 23.Boy:Yes.\n 24.Boy:I have always thought of him as being very young, but, if I could handle the chores at his age, I am sure he could manage too. Maybe we could take turns with the cooking and other jobs. That would be another way for me to get more rest and more time for study.\n 25.Boy:First I need to get more rest and find better times for study. I will stay after school for about an hour so I can read in the daylight. Then at weekends I will go to the farm to read. At home I will get my younger brother to take turns with me in doing the cooking and other chores.\n 26.Boy:I will. Thank you for your help. Good bye.\n";
        script_2 = "Provider: So, Mr. Smith, I got a report from the ER doctor about what happened, but I'd like to  hear it directly from you in your own words. What happened this morning that brought you in to the ER? \n 1.Patient: Well, I had just been feeling very anxious. \n 2.Patient: I started feeling worse a few days ago, but last night it got really bad.\n 3.Patient: The thing is that all night long, I was feeling nervous and jittery. I just couldn't relax. My wife was really worried. I tried to go to bed and fall asleep, but I couldn't. Then, all of a sudden, I felt short of breath. I was gasping for air as if I were drowning...\n 4.Patient: Yeah, I had a terrible headache back here in the nape of my neck.\n 5.Patient: At work, I couldn't concentrate at all. I was irritable with everyone. My nerves were shot. Every little thing would startle me. I didn't have any appetite. Whenever I would eat it would make me nauseous. But the worst thing is that I couldn't sleep at all. I think it's been like four nights now that I've hardly gotten any sleep.\n 6.Patient: Yeah, he gave me two pills, one was for a depression. I think it was called Zoloft.\n 7.Patient: It was a little round one that was called loraza something...\n 8.Patient: Yeah, that's it. \n 9.A little bit. The lorazepam one, the doctor took away. The other one I just stopped taking.\n 10.Patient: The thing is that I took it for several days and it really didn't seem like it was helping much. It just made me feel tired and numb all the time. I don't know how to describe it. It was hard to concentrate at work. I was scared that I was going to get fired and I really can't afford to lose my job, so I just decided not to take it anymore. \n 11.Patient: OK. \n 12.Patient: The problem is that because I missed a few appointments, supposedly that psychiatrist doesn't want to see me anymore. \n 13.Patient: That should be OK. I will try to do better this time getting to my appointments.  Thanks for everything.\n";

        script.text = scriptOption == Scenario.script1 ? script_1 : script_2;
        initialYpos = autoScroll.content.localPosition.y;
        yPos = initialYpos;
    }
    // Update is called once per frame
    void Update()
    {
        if (AvatarNetworkManager.connectionStatus == AvatarNetworkManager.connectionStatuses.Connected || 
            AvatarNetworkManager.connectionStatus == AvatarNetworkManager.connectionStatuses.ConnectedHost)
        {
            if (yPos > -1f)
            {
                yPos = initialYpos;
            }
            yPos += Time.deltaTime * scrollingSpeed;
            autoScroll.content.localPosition = new Vector2(0, yPos);
        }
    }
}
