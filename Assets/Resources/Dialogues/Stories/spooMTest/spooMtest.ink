VAR storageTier = 0 //just your hands and shallow pockets
VAR availStorage = 4
LIST inventoryConts = (Honeycake), (Map), (Rockblossom), (A_Really_Neat_Stick), (Waybread)
~inventoryConts = ()
->BEGIN
===BEGIN

The Baker, Barbra, smiles warmly, busying herself with today's bakes.

* "How's business been?"
    She smiles, grateful for the inquiry "Oh, you know, more than necessary, less than I'd like" She chuckles
    ->BEGIN
    
* "Whatcha got planned for today?"
    <i>She sighs, looking at her workbench</i>
    "Seven loaves, twenty rolls, a honeycake for the mayor, and some waybread for Arnt."
        ->BakeryQs
==BakeryQs    

* ["A honeycake?"]
<i>Your eyebrows shoot up, it's been a long time since you've had one of those.</i> 
"Any special occasion for the Mayor?"
    <i>She shrugs.</i> "Nothing elaborate, more of an oversized sweetroll. You can run it down to him, if you like."
    ** ["Yeah, for sure."] <i>You nod eagerly.</i>
    ~    availStorage = availStorage - 1
    
    ~inventoryConts += Honeycake
        "Very good, dear. Keep a spring in your step, he may live right up the road but you never know when you can catch him at home."
    ->BakeryQs
    ** ["I've got some other things to take care of."]
    
    "That's quite alright, dear."
    ->BakeryQs

*  ["Did you say Arnt?"]
<i>She nods firmly.</i> "The Ranger, not that..." <i>She trails off absent mindedly.</i>
    ** ["...you were saying?"] <i>You stare at her until she reacts.</i>
        <i>She blinks, only now realizing she was speaking out loud.</i>
        "Arnt lives out in the Nearwood and I haven't seen hide nor hair of Jent."
        <i>You know Jent. Lithe and lean, he helps out by running light loads to nearby outposts and neighboringring villages. </i>
        ***["Yeah, it's been a while since I've seen him."]"Yeah, it's been a while since I've seen him."
            <i>She nods.</i> "Last I heard he was on his way to Varse," <i>the next village over, by way of the Nearwood</i> "but he should have been well back by now."
            ****[Have a good think.] ->AGoodThink
            
            
===AGoodThink
+[On One Hand]
    Any chance to get in good with Barbra is one worth taking. You've never really left the village, but people from all over are sending couriors just to get some of her specialties.
    ++[But on the other...]
        Never leaving the village means you don't have much experience with the wilderness, let alone the Nearwood.
        +++ So what'll it be?
            **** [Take the Quest]
              <i>Barbara stops and looks at you, eyeing you up and down.<i/>
              "You sure? It can get awful dicey out there for the unwary."
              ***** [Lean into her concerns.]
                "I know, but also knowing that will keep me on my toes!
                ->BeginQuest
              ***** [Bluster through.]
                "I know I look green, but I've more than got this!"
                ->BeginQuest
            ++++ [Think better of it.]
            ->ThoughtBetter
==BeginQuest
    ~inventoryConts += Waybread
<i>She nods, dismissing her concerns.<i/>
"I wouldn't normally send you off, dear, but Arnt's shipment is already a few days late.
Don't forget your pack and supplies, it could take a day or two to get to Arnt's hut."
->HeadOff
=HeadOff
Your first task begins!
->END
==ThoughtBetter
Better to ere on the side of safety. Who KNOWS what sorts of dangers lurk in the wilderness beyond your village.
That thought sticks with you for the rest of the day, the rest of the week, and on into the whole rest of the year. It worms its way into your heart and mind, becoming a core part of your worldview.
You never leave the village, but you live a safe life because of it. It's somewhat happy...
...at least, that's what you always assumed.

    -> END
