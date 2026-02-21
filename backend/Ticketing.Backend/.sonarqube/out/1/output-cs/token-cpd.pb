√A
lC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Services\ITicketService.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
Services  (
;( )
public 
	interface 
ITicketService 
{ 
Task 
< 	
IEnumerable	 
< 
TicketResponse #
># $
>$ %
GetTicketsAsync& 5
(5 6
Guid6 :
userId; A
,A B
UserRoleC K
roleL P
,P Q
TicketStatusR ^
?^ _
status` f
,f g
TicketPriorityh v
?v w
priority	x Ä
,
Ä Å
Guid
Ç Ü
?
Ü á

assignedTo
à í
,
í ì
Guid
î ò
?
ò ô
	createdBy
ö £
,
£ §
string
• ´
?
´ ¨
search
≠ ≥
)
≥ ¥
;
¥ µ
Task		 
<		 	
IEnumerable			 
<		 
TicketResponse		 #
>		# $
>		$ %%
GetTechnicianTicketsAsync		& ?
(		? @
Guid		@ D
technicianUserId		E U
,		U V
string		W ]
?		] ^
mode		_ c
=		d e
null		f j
)		j k
;		k l
Task

 
<

 	
TicketResponse

	 
?

 
>

 
GetTicketAsync

 (
(

( )
Guid

) -
id

. 0
,

0 1
Guid

2 6
userId

7 =
,

= >
UserRole

? G
role

H L
)

L M
;

M N
Task 
< 	
TicketResponse	 
? 
> 
CreateTicketAsync +
(+ ,
Guid, 0
userId1 7
,7 8
TicketCreateRequest9 L
requestM T
,T U
ListV Z
<Z [
DTOs[ _
._ `!
FileAttachmentRequest` u
>u v
?v w
attachments	x É
=
Ñ Ö
null
Ü ä
)
ä ã
;
ã å
Task 
< 	
TicketResponse	 
? 
> 
UpdateTicketAsync +
(+ ,
Guid, 0
id1 3
,3 4
Guid5 9
userId: @
,@ A
UserRoleB J
roleK O
,O P
TicketUpdateRequestQ d
requeste l
)l m
;m n
Task 
< 	
TicketResponse	 
? 
> 
AssignTicketAsync +
(+ ,
Guid, 0
id1 3
,3 4
Guid5 9
technicianId: F
)F G
;G H
Task 
< 	
IEnumerable	 
< 
TicketMessageDto %
>% &
>& '
GetMessagesAsync( 8
(8 9
Guid9 =
ticketId> F
,F G
GuidH L
userIdM S
,S T
UserRoleU ]
role^ b
)b c
;c d
Task 
< 	
TicketMessageDto	 
? 
> 
AddMessageAsync +
(+ ,
Guid, 0
ticketId1 9
,9 :
Guid; ?
authorId@ H
,H I
stringJ P
messageQ X
,X Y
TicketStatusZ f
?f g
statush n
=o p
nullq u
)u v
;v w
Task 
< 	
IEnumerable	 
< "
TicketCalendarResponse +
>+ ,
>, -#
GetCalendarTicketsAsync. E
(E F
DateTimeF N
	startDateO X
,X Y
DateTimeZ b
endDatec j
)j k
;k l
Task 
< 	#
AssignmentQueueResponse	  
>  !#
GetAssignmentQueueAsync" 9
(9 :
string: @
?@ A
typeB F
=G H
nullI M
,M N
TicketStatusO [
?[ \
status] c
=d e
nullf j
,j k
intl o
?o p
pageq u
=v w
nullx |
,| }
int	~ Å
?
Å Ç
pageSize
É ã
=
å ç
null
é í
)
í ì
;
ì î
Task 
< 	
List	 
< 
TicketTechnicianDto !
>! "
>" #"
AssignTechniciansAsync$ :
(: ;
Guid; ?
ticketId@ H
,H I
ListJ N
<N O
GuidO S
>S T
technicianIdsU b
,b c
Guidd h
?h i
leadTechnicianIdj z
,z {
Guid	| Ä
actorUserId
Å å
)
å ç
;
ç é
Task 
< 	
bool	 
> !
RemoveTechnicianAsync $
($ %
Guid% )
ticketId* 2
,2 3
Guid4 8
technicianId9 E
,E F
GuidG K
actorUserIdL W
)W X
;X Y
Task 
< 	
List	 
< 
TicketTechnicianDto !
>! "
>" #%
GetTicketTechniciansAsync$ =
(= >
Guid> B
ticketIdC K
,K L
GuidM Q
userIdR X
,X Y
UserRoleZ b
rolec g
)g h
;h i
Task 
< 	
TicketTechnicianDto	 
? 
> &
UpdateTechnicianStateAsync 9
(9 :
Guid: >
ticketId? G
,G H
GuidI M
technicianUserIdN ^
,^ _!
TicketTechnicianState` u
newStatev ~
)~ 
;	 Ä
Task 
< 	
List	 
< 
TicketActivityDto 
>  
>  !$
GetTicketActivitiesAsync" :
(: ;
Guid; ?
ticketId@ H
,H I
GuidJ N
userIdO U
,U V
UserRoleW _
role` d
)d e
;e f
Task "
UpdateWorkSessionAsync	 
(  
Guid  $
ticketId% -
,- .
Guid/ 3
technicianUserId4 D
,D E$
UpdateWorkSessionRequestF ^
request_ f
)f g
;g h
Task 
< 	'
TicketCollaborationResponse	 $
?$ %
>% &%
GetCollaborationDataAsync' @
(@ A
GuidA E
ticketIdF N
,N O
GuidP T
userIdU [
,[ \
UserRole] e
rolef j
)j k
;k l
Task 
< 	
bool	 
> )
SetResponsibleTechnicianAsync ,
(, -
Guid- 1
ticketId2 :
,: ;
Guid< @#
responsibleTechnicianIdA X
,X Y
GuidZ ^
actorUserId_ j
,j k
UserRolel t
	actorRoleu ~
)~ 
;	 Ä
Task"" 
<"" 	
TicketResponse""	 
?"" 
>"" #
UpdateTicketStatusAsync"" 1
(""1 2
Guid""2 6
ticketId""7 ?
,""? @
Guid""A E
userId""F L
,""L M
UserRole""N V
role""W [
,""[ \
TicketStatus""] i
	newStatus""j s
)""s t
;""t u
Task%% 
<%% 	
TicketResponse%%	 
?%% 
>%% 
HandoffTicketAsync%% ,
(%%, -
Guid%%- 1
ticketId%%2 :
,%%: ;
Guid%%< @ 
fromTechnicianUserId%%A U
,%%U V
Guid%%W [
toTechnicianId%%\ j
,%%j k
string%%l r
?%%r s
reason%%t z
,%%z {
string	%%| Ç
?
%%Ç É
note
%%Ñ à
,
%%à â
Guid
%%ä é
actorUserId
%%è ö
,
%%ö õ
UserRole
%%ú §
	actorRole
%%• Æ
)
%%Æ Ø
;
%%Ø ∞
}&& ·
pC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Services\ITechnicianService.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
Services  (
;( )
public 
	interface 
ITechnicianService #
{$ %
}& '
tC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Services\ISystemSettingsService.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
Services  (
;( )
public 
	interface "
ISystemSettingsService '
{ 
Task 
< 	"
SystemSettingsResponse	 
>  "
GetSystemSettingsAsync! 7
(7 8
)8 9
;9 :
Task 
< 	"
SystemSettingsResponse	 
>  %
UpdateSystemSettingsAsync! :
(: ;'
SystemSettingsUpdateRequest; V
requestW ^
)^ _
;_ `
}		 Î
uC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Services\ISmartAssignmentService.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
Services  (
;( )
public 
	interface #
ISmartAssignmentService (
{) *
}+ ,√
qC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Repositories\IUserRepository.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
Repositories  ,
;, -
public 
	interface 
IUserRepository  
{ 
Task 
< 	
User	 
? 
> 
GetByIdAsync 
( 
Guid !
id" $
)$ %
;% &
Task 
< 	
User	 
? 
> 
GetByEmailAsync 
(  
string  &
email' ,
), -
;- .
Task		 
<		 	
IEnumerable			 
<		 
User		 
>		 
>		 
GetAllAsync		 '
(		' (
)		( )
;		) *
Task

 
<

 	
User

	 
>

 
AddAsync

 
(

 
User

 
user

 !
)

! "
;

" #
Task 
< 	
User	 
> 
UpdateAsync 
( 
User 
user  $
)$ %
;% &
Task 
< 	
bool	 
> 
DeleteAsync 
( 
Guid 
id  "
)" #
;# $
Task 
< 	
bool	 
> 
ExistsAsync 
( 
Guid 
id  "
)" #
;# $
} ˇ
|C:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Repositories\IUserPreferencesRepository.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
Repositories  ,
;, -
public 
	interface &
IUserPreferencesRepository +
{ 
Task 
< 	
UserPreferences	 
? 
> 
GetByUserIdAsync +
(+ ,
Guid, 0
userId1 7
)7 8
;8 9
Task 
< 	
UserPreferences	 
> 
AddAsync "
(" #
UserPreferences# 2
preferences3 >
)> ?
;? @
Task		 
<		 	
UserPreferences			 
>		 
UpdateAsync		 %
(		% &
UserPreferences		& 5
preferences		6 A
)		A B
;		B C
Task

 
<

 	
bool

	 
>

 
DeleteAsync

 
(

 
Guid

 
userId

  &
)

& '
;

' (
} ˇ
mC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Repositories\IUnitOfWork.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
Repositories  ,
;, -
public 
	interface 
IUnitOfWork 
{ 
ITicketRepository		 
Tickets		 
{		 
get		  #
;		# $
}		% &
IUserRepository

 
Users

 
{

 
get

 
;

  
}

! "
ICategoryRepository 

Categories "
{# $
get% (
;( )
}* +!
ITechnicianRepository 
Technicians %
{& '
get( +
;+ ,
}- .%
ITicketActivityRepository 
TicketActivities .
{/ 0
get1 4
;4 5
}6 7$
ITicketMessageRepository 
TicketMessages +
{, -
get. 1
;1 2
}3 4'
ITicketTechnicianRepository 
TicketTechnicians  1
{2 3
get4 7
;7 8
}9 :(
ITicketWorkSessionRepository  
TicketWorkSessions! 3
{4 5
get6 9
;9 :
}; <%
ISystemSettingsRepository 
SystemSettings ,
{- .
get/ 2
;2 3
}4 5&
IUserPreferencesRepository 
UserPreferences .
{/ 0
get1 4
;4 5
}6 7&
IFieldDefinitionRepository 
FieldDefinitions /
{0 1
get2 5
;5 6
}7 8
Task 
< 	
int	 
> 
SaveChangesAsync 
( 
CancellationToken 0
cancellationToken1 B
=C D
defaultE L
)L M
;M N
Task !
BeginTransactionAsync	 
( 
CancellationToken 0
cancellationToken1 B
=C D
defaultE L
)L M
;M N
Task "
CommitTransactionAsync	 
(  
CancellationToken  1
cancellationToken2 C
=D E
defaultF M
)M N
;N O
Task $
RollbackTransactionAsync	 !
(! "
CancellationToken" 3
cancellationToken4 E
=F G
defaultH O
)O P
;P Q
} ﬂ
~C:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Repositories\ITicketWorkSessionRepository.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
Repositories  ,
;, -
public 
	interface (
ITicketWorkSessionRepository -
{ 
Task 
< 	
TicketWorkSession	 
? 
> 
GetByIdAsync )
() *
Guid* .
id/ 1
)1 2
;2 3
Task 
< 	
IEnumerable	 
< 
TicketWorkSession &
>& '
>' (
GetByTicketIdAsync) ;
(; <
Guid< @
ticketIdA I
)I J
;J K
Task		 
<		 	
TicketWorkSession			 
?		 
>		 )
GetByTicketAndTechnicianAsync		 :
(		: ;
Guid		; ?
ticketId		@ H
,		H I
Guid		J N
technicianUserId		O _
)		_ `
;		` a
Task

 
<

 	
TicketWorkSession

	 
>

 
AddAsync

 $
(

$ %
TicketWorkSession

% 6
session

7 >
)

> ?
;

? @
Task 
< 	
TicketWorkSession	 
> 
UpdateAsync '
(' (
TicketWorkSession( 9
session: A
)A B
;B C
Task 
< 	
bool	 
> 
DeleteAsync 
( 
Guid 
id  "
)" #
;# $
Task 
< 	
TicketWorkSession	 
> 
AddOrUpdateAsync ,
(, -
TicketWorkSession- >
session? F
)F G
;G H
} ƒ
}C:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Repositories\ITicketTechnicianRepository.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
Repositories  ,
;, -
public 
	interface '
ITicketTechnicianRepository ,
{ 
Task 
< 	
TicketTechnician	 
? 
> 
GetByIdAsync (
(( )
Guid) -
id. 0
)0 1
;1 2
Task 
< 	
TicketTechnician	 
? 
> )
GetByTicketAndTechnicianAsync 9
(9 :
Guid: >
ticketId? G
,G H
GuidI M
technicianUserIdN ^
)^ _
;_ `
Task		 
<		 	
TicketTechnician			 
?		 
>		 +
GetByTicketAndTechnicianIdAsync		 ;
(		; <
Guid		< @
ticketId		A I
,		I J
Guid		K O
technicianId		P \
)		\ ]
;		] ^
Task

 
<

 	
IEnumerable

	 
<

 
TicketTechnician

 %
>

% &
>

& '
GetByTicketIdAsync

( :
(

: ;
Guid

; ?
ticketId

@ H
)

H I
;

I J
Task 
< 	
IEnumerable	 
< 
TicketTechnician %
>% &
>& '&
GetByTechnicianUserIdAsync( B
(B C
GuidC G
technicianUserIdH X
)X Y
;Y Z
Task 
< 	
IEnumerable	 
< 
TicketTechnician %
>% &
>& '"
GetByTechnicianIdAsync( >
(> ?
Guid? C
technicianIdD P
)P Q
;Q R
Task 
< 	
TicketTechnician	 
> 
AddAsync #
(# $
TicketTechnician$ 4
ticketTechnician5 E
)E F
;F G
Task 
< 	
TicketTechnician	 
> 
UpdateAsync &
(& '
TicketTechnician' 7
ticketTechnician8 H
)H I
;I J
Task 
< 	
bool	 
> 
DeleteAsync 
( 
Guid 
id  "
)" #
;# $
Task 
< 	
bool	 
> ,
 DeleteByTicketAndTechnicianAsync /
(/ 0
Guid0 4
ticketId5 =
,= >
Guid? C
technicianUserIdD T
)T U
;U V
} ≤
sC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Repositories\ITicketRepository.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
Repositories  ,
;, -
public 
	interface 
ITicketRepository "
{ 
Task 
< 	
Ticket	 
? 
> 
GetByIdAsync 
( 
Guid #
id$ &
)& '
;' (
Task		 
<		 	
Ticket			 
?		 
>		 $
GetByIdWithIncludesAsync		 *
(		* +
Guid		+ /
id		0 2
)		2 3
;		3 4
Task

 
<

 	
IEnumerable

	 
<

 
Ticket

 
>

 
>

 
GetTicketsAsync

 -
(

- .
UserRole 
role 
, 
Guid 
userId 
, 
TicketStatus 
? 
status 
= 
null #
,# $
TicketPriority 
? 
priority  
=! "
null# '
,' (
Guid 
? 

assignedTo 
= 
null 
,  
Guid 
? 
	createdBy 
= 
null 
, 
string 
? 
search 
= 
null 
) 
; 
Task 
< 	
Ticket	 
> 
AddAsync 
( 
Ticket  
ticket! '
)' (
;( )
Task 
< 	
Ticket	 
> 
UpdateAsync 
( 
Ticket #
ticket$ *
)* +
;+ ,
Task 
< 	
bool	 
> 
DeleteAsync 
( 
Guid 
id  "
)" #
;# $
Task 
< 	
IEnumerable	 
< 
Ticket 
> 
> #
GetCalendarTicketsAsync 5
(5 6
DateTime6 >
	startDate? H
,H I
DateTimeJ R
endDateS Z
)Z [
;[ \
Task 
< 	
int	 
> #
GetUnassignedCountAsync %
(% &
)& '
;' (
Task 
< 	
Ticket	 
? 
> 
GetBasicByIdAsync #
(# $
Guid$ (
id) +
)+ ,
;, -
} ‘

zC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Repositories\ITicketMessageRepository.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
Repositories  ,
;, -
public 
	interface $
ITicketMessageRepository )
{ 
Task 
< 	
TicketMessage	 
? 
> 
GetByIdAsync %
(% &
Guid& *
id+ -
)- .
;. /
Task 
< 	
IEnumerable	 
< 
TicketMessage "
>" #
># $
GetByTicketIdAsync% 7
(7 8
Guid8 <
ticketId= E
)E F
;F G
Task		 
<		 	
TicketMessage			 
>		 
AddAsync		  
(		  !
TicketMessage		! .
message		/ 6
)		6 7
;		7 8
Task

 
<

 	
TicketMessage

	 
>

 
UpdateAsync

 #
(

# $
TicketMessage

$ 1
message

2 9
)

9 :
;

: ;
Task 
< 	
bool	 
> 
DeleteAsync 
( 
Guid 
id  "
)" #
;# $
} Ï
{C:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Repositories\ITicketActivityRepository.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
Repositories  ,
;, -
public 
	interface %
ITicketActivityRepository *
{ 
Task 
< 	
TicketActivity	 
? 
> 
GetByIdAsync &
(& '
Guid' +
id, .
). /
;/ 0
Task 
< 	
IEnumerable	 
< 
TicketActivity #
># $
>$ %
GetByTicketIdAsync& 8
(8 9
Guid9 =
ticketId> F
)F G
;G H
Task		 
<		 	
TicketActivity			 
>		 
AddAsync		 !
(		! "
TicketActivity		" 0
activity		1 9
)		9 :
;		: ;
Task

 
<

 	
bool

	 
>

 
DeleteAsync

 
(

 
Guid

 
id

  "
)

" #
;

# $
Task 
< 	
IEnumerable	 
< 
TicketActivity #
># $
>$ %$
GetRecentByTicketIdAsync& >
(> ?
Guid? C
ticketIdD L
,L M
intN Q
countR W
=X Y
$numZ \
)\ ]
;] ^
} ª
wC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Repositories\ITechnicianRepository.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
Repositories  ,
;, -
public 
	interface !
ITechnicianRepository &
{ 
Task 
< 	

Technician	 
? 
> 
GetByIdAsync "
(" #
Guid# '
id( *
)* +
;+ ,
Task 
< 	

Technician	 
? 
> $
GetByIdWithIncludesAsync .
(. /
Guid/ 3
id4 6
)6 7
;7 8
Task		 
<		 	
IEnumerable			 
<		 

Technician		 
>		  
>		  !
GetAllAsync		" -
(		- .
)		. /
;		/ 0
Task

 
<

 	
IEnumerable

	 
<

 

Technician

 
>

  
>

  !
GetActiveAsync

" 0
(

0 1
)

1 2
;

2 3
Task 
< 	

Technician	 
? 
> 
GetByUserIdAsync &
(& '
Guid' +
userId, 2
)2 3
;3 4
Task 
< 	

Technician	 
> 
AddAsync 
( 

Technician (

technician) 3
)3 4
;4 5
Task 
< 	

Technician	 
> 
UpdateAsync  
(  !

Technician! +

technician, 6
)6 7
;7 8
Task 
< 	
bool	 
> 
DeleteAsync 
( 
Guid 
id  "
)" #
;# $
Task 
< 	
bool	 
> #
HasAssignedTicketsAsync &
(& '
Guid' +
technicianId, 8
)8 9
;9 :
Task 
< 	
IEnumerable	 
< 
Guid 
> 
> 2
&GetTechnicianUserIdsBySubcategoryAsync B
(B C
intC F
subcategoryIdG T
)T U
;U V
Task -
!UpdateSubcategoryPermissionsAsync	 *
(* +
Guid+ /
technicianId0 <
,< =
List> B
<B C
intC F
>F G
subcategoryIdsH V
)V W
;W X
} ˜
{C:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Repositories\ISystemSettingsRepository.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
Repositories  ,
;, -
public 
	interface %
ISystemSettingsRepository *
{ 
Task 
< 	
SystemSettings	 
? 
> 
GetByIdAsync &
(& '
int' *
id+ -
)- .
;. /
Task 
< 	
SystemSettings	 
? 
> 
GetByNameAsync (
(( )
string) /
name0 4
)4 5
;5 6
Task		 
<		 	
IEnumerable			 
<		 
SystemSettings		 #
>		# $
>		$ %
GetAllAsync		& 1
(		1 2
)		2 3
;		3 4
Task

 
<

 	
SystemSettings

	 
>

 
AddAsync

 !
(

! "
SystemSettings

" 0
settings

1 9
)

9 :
;

: ;
Task 
< 	
SystemSettings	 
> 
UpdateAsync $
($ %
SystemSettings% 3
settings4 <
)< =
;= >
Task 
< 	
bool	 
> 
DeleteAsync 
( 
int 
id !
)! "
;" #
} ú
|C:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Repositories\IFieldDefinitionRepository.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
Repositories  ,
;, -
public 
	interface &
IFieldDefinitionRepository +
{ 
Task 
< 	&
SubcategoryFieldDefinition	 #
?# $
>$ %
GetByIdAsync& 2
(2 3
int3 6
id7 9
)9 :
;: ;
Task 
< 	
IEnumerable	 
< &
SubcategoryFieldDefinition /
>/ 0
>0 1#
GetBySubcategoryIdAsync2 I
(I J
intJ M
subcategoryIdN [
,[ \
bool] a
includeInactiveb q
=r s
truet x
)x y
;y z
Task		 
<		 	&
SubcategoryFieldDefinition			 #
>		# $
AddAsync		% -
(		- .&
SubcategoryFieldDefinition		. H
fieldDefinition		I X
)		X Y
;		Y Z
Task

 
<

 	&
SubcategoryFieldDefinition

	 #
>

# $
UpdateAsync

% 0
(

0 1&
SubcategoryFieldDefinition

1 K
fieldDefinition

L [
)

[ \
;

\ ]
Task 
< 	
bool	 
> 
DeleteAsync 
( 
int 
id !
)! "
;" #
} »
uC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Repositories\ICategoryRepository.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
Repositories  ,
;, -
public 
	interface 
ICategoryRepository $
{ 
Task 
< 	
Category	 
? 
> 
GetByIdAsync  
(  !
int! $
id% '
)' (
;( )
Task 
< 	
Subcategory	 
? 
> #
GetSubcategoryByIdAsync .
(. /
int/ 2
id3 5
)5 6
;6 7
Task		 
<		 	
IEnumerable			 
<		 
Category		 
>		 
>		 
GetAllAsync		  +
(		+ ,
)		, -
;		- .
Task

 
<

 	
Category

	 
>

 
AddAsync

 
(

 
Category

 $
category

% -
)

- .
;

. /
Task 
< 	
Category	 
> 
UpdateAsync 
( 
Category '
category( 0
)0 1
;1 2
Task 
< 	
bool	 
> 
DeleteAsync 
( 
int 
id !
)! "
;" #
} ˘
uC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Exceptions\UnauthorizedException.cs
	namespace 	
	Ticketing
 
. 
Application 
.  

Exceptions  *
;* +
public 
class !
UnauthorizedException "
:# $
	Exception% .
{ 
public 
!
UnauthorizedException  
(  !
string! '
message( /
)/ 0
:1 2
base3 7
(7 8
message8 ?
)? @
{A B
}C D
} ‡
yC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Exceptions\TicketValidationException.cs
	namespace 	
	Ticketing
 
. 
Application 
.  

Exceptions  *
;* +
public 
class %
TicketValidationException &
:' (
	Exception) 2
{ 
public 
%
TicketValidationException $
($ %
string% +
message, 3
)3 4
:5 6
base7 ;
(; <
message< C
)C D
{E F
}G H
public 
%
TicketValidationException $
($ %
string% +
message, 3
,3 4
	Exception5 >
innerException? M
)M N
:O P
baseQ U
(U V
messageV ]
,] ^
innerException_ m
)m n
{o p
}q r
public 
%
TicketValidationException $
($ %
string% +
code, 0
,0 1
string2 8
message9 @
,@ A
objectB H
?H I
fieldJ O
=P Q
nullR V
,V W
objectX ^
?^ _
value` e
=f g
nullh l
)l m
:n o
basep t
(t u
messageu |
)| }
{ 
Code		 
=		 
code		 
;		 
Field

 
=

 
field

 
?

 
.

 
ToString

 
(

  
)

  !
;

! "
Value 
= 
value 
? 
. 
ToString 
(  
)  !
;! "
} 
public 

string 
? 
Code 
{ 
get 
; 
}  
public 

string 
? 
Field 
{ 
get 
; 
}  !
public 

string 
? 
Value 
{ 
get 
; 
}  !
} î
~C:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Exceptions\StatusChangeForbiddenException.cs
	namespace 	
	Ticketing
 
. 
Application 
.  

Exceptions  *
;* +
public 
class *
StatusChangeForbiddenException +
:, -
	Exception. 7
{ 
public 
*
StatusChangeForbiddenException )
() *
string* 0
message1 8
)8 9
:: ;
base< @
(@ A
messageA H
)H I
{J K
}L M
} Ì
qC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Exceptions\NotFoundException.cs
	namespace 	
	Ticketing
 
. 
Application 
.  

Exceptions  *
;* +
public 
class 
NotFoundException 
:  
	Exception! *
{ 
public 

NotFoundException 
( 
string #
message$ +
)+ ,
:- .
base/ 3
(3 4
message4 ;
); <
{= >
}? @
} Â*
mC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\DTOs\UserPreferencesDtos.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
DTOs  $
;$ %
public 
class #
UserPreferencesResponse $
{ 
public 

string 
Theme 
{ 
get 
; 
set "
;" #
}$ %
=& '
$str( 0
;0 1
public 

string 
FontSize 
{ 
get  
;  !
set" %
;% &
}' (
=) *
$str+ /
;/ 0
public		 

string		 
Language		 
{		 
get		  
;		  !
set		" %
;		% &
}		' (
=		) *
$str		+ /
;		/ 0
public

 

string

 
	Direction

 
{

 
get

 !
;

! "
set

# &
;

& '
}

( )
=

* +
$str

, 1
;

1 2
public 

string 
Timezone 
{ 
get  
;  !
set" %
;% &
}' (
=) *
$str+ 8
;8 9
public 
+
NotificationPreferencesResponse *
Notifications+ 8
{9 :
get; >
;> ?
set@ C
;C D
}E F
=G H
newI L
(L M
)M N
;N O
} 
public 
class +
NotificationPreferencesResponse ,
{ 
public 

bool 
EmailEnabled 
{ 
get "
;" #
set$ '
;' (
}) *
=+ ,
true- 1
;1 2
public 

bool 
PushEnabled 
{ 
get !
;! "
set# &
;& '
}( )
=* +
true, 0
;0 1
public 

bool 

SmsEnabled 
{ 
get  
;  !
set" %
;% &
}' (
=) *
false+ 0
;0 1
public 

bool 
DesktopEnabled 
{  
get! $
;$ %
set& )
;) *
}+ ,
=- .
true/ 3
;3 4
} 
public 
class 0
$NotificationPreferencesUpdateRequest 1
{ 
[ 
Required 
] 
public 

bool 
EmailEnabled 
{ 
get "
;" #
set$ '
;' (
}) *
=+ ,
true- 1
;1 2
[ 
Required 
] 
public 

bool 
PushEnabled 
{ 
get !
;! "
set# &
;& '
}( )
=* +
true, 0
;0 1
[ 
Required 
] 
public   

bool   

SmsEnabled   
{   
get    
;    !
set  " %
;  % &
}  ' (
=  ) *
false  + 0
;  0 1
["" 
Required"" 
]"" 
public## 

bool## 
DesktopEnabled## 
{##  
get##! $
;##$ %
set##& )
;##) *
}##+ ,
=##- .
true##/ 3
;##3 4
}$$ 
public&& 
class&& (
UserPreferencesUpdateRequest&& )
{'' 
[(( 
Required(( 
](( 
[)) 
RegularExpression)) 
()) 
$str)) .
,)). /
ErrorMessage))0 <
=))= >
$str))? e
)))e f
]))f g
public** 

string** 
Theme** 
{** 
get** 
;** 
set** "
;**" #
}**$ %
=**& '
$str**( 0
;**0 1
[,, 
Required,, 
],, 
[-- 
RegularExpression-- 
(-- 
$str-- %
,--% &
ErrorMessage--' 3
=--4 5
$str--6 V
)--V W
]--W X
public.. 

string.. 
FontSize.. 
{.. 
get..  
;..  !
set.." %
;..% &
}..' (
=..) *
$str..+ /
;../ 0
[00 
Required00 
]00 
[11 
RegularExpression11 
(11 
$str11 "
,11" #
ErrorMessage11$ 0
=111 2
$str113 N
)11N O
]11O P
public22 

string22 
Language22 
{22 
get22  
;22  !
set22" %
;22% &
}22' (
=22) *
$str22+ /
;22/ 0
[44 
Required44 
]44 
[55 
	MaxLength55 
(55 
$num55 
)55 
]55 
public66 

string66 
Timezone66 
{66 
get66  
;66  !
set66" %
;66% &
}66' (
=66) *
$str66+ 8
;668 9
}77 ñÖ
dC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\DTOs\TicketDtos.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
DTOs  $
;$ %
public 
class %
TicketDynamicFieldRequest &
{ 
[ 
Required 
] 
public		 

int		 
FieldDefinitionId		  
{		! "
get		# &
;		& '
set		( +
;		+ ,
}		- .
[ 
Required 
] 
public 

string 
Value 
{ 
get 
; 
set "
;" #
}$ %
=& '
string( .
.. /
Empty/ 4
;4 5
} 
public 
class 
TicketCreateRequest  
{ 
[ 
Required 
( 
ErrorMessage 
= 
$str 0
)0 1
]1 2
[ 
StringLength 
( 
$num 
, 
MinimumLength $
=% &
$num' (
,( )
ErrorMessage* 6
=7 8
$str9 e
)e f
]f g
public 

string 
Title 
{ 
get 
; 
set "
;" #
}$ %
=& '
string( .
.. /
Empty/ 4
;4 5
[ 
Required 
( 
ErrorMessage 
= 
$str 6
)6 7
]7 8
[ 
StringLength 
( 
$num 
, 
MinimumLength %
=& '
$num( *
,* +
ErrorMessage, 8
=9 :
$str; o
)o p
]p q
public 

string 
Description 
{ 
get  #
;# $
set% (
;( )
}* +
=, -
string. 4
.4 5
Empty5 :
;: ;
[ 
Required 
( 
ErrorMessage 
= 
$str 5
)5 6
]6 7
[ 
Range 

(
 
$num 
, 
int 
. 
MaxValue 
, 
ErrorMessage (
=) *
$str+ R
)R S
]S T
public 

int 

CategoryId 
{ 
get 
;  
set! $
;$ %
}& '
[ 
Range 

(
 
$num 
, 
int 
. 
MaxValue 
, 
ErrorMessage (
=) *
$str+ a
)a b
]b c
public 

int 
? 
SubcategoryId 
{ 
get  #
;# $
set% (
;( )
}* +
[   
Required   
(   
ErrorMessage   
=   
$str   3
)  3 4
]  4 5
public!! 

TicketPriority!! 
Priority!! "
{!!# $
get!!% (
;!!( )
set!!* -
;!!- .
}!!/ 0
public## 

List## 
<## %
TicketDynamicFieldRequest## )
>##) *
?##* +
DynamicFields##, 9
{##: ;
get##< ?
;##? @
set##A D
;##D E
}##F G
}$$ 
public&& 
class&& 
TicketUpdateRequest&&  
{'' 
public(( 

TicketStatus(( 
?(( 
Status(( 
{((  !
get((" %
;((% &
set((' *
;((* +
}((, -
public)) 

TicketPriority)) 
?)) 
Priority)) #
{))$ %
get))& )
;))) *
set))+ .
;)). /
}))0 1
public** 

Guid** 
?** 
AssignedToUserId** !
{**" #
get**$ '
;**' (
set**) ,
;**, -
}**. /
public++ 

DateTime++ 
?++ 
DueDate++ 
{++ 
get++ "
;++" #
set++$ '
;++' (
}++) *
public,, 

string,, 
?,, 
Description,, 
{,,  
get,,! $
;,,$ %
set,,& )
;,,) *
},,+ ,
}-- 
public22 
class22 !
FileAttachmentRequest22 "
{33 
public44 

string44 
FileName44 
{44 
get44  
;44  !
set44" %
;44% &
}44' (
=44) *
string44+ 1
.441 2
Empty442 7
;447 8
public55 

string55 
ContentType55 
{55 
get55  #
;55# $
set55% (
;55( )
}55* +
=55, -
string55. 4
.554 5
Empty555 :
;55: ;
public66 

byte66 
[66 
]66 
Content66 
{66 
get66 
;66  
set66! $
;66$ %
}66& '
=66( )
Array66* /
.66/ 0
Empty660 5
<665 6
byte666 :
>66: ;
(66; <
)66< =
;66= >
public77 

long77 
FileSize77 
{77 
get77 
;77 
set77  #
;77# $
}77% &
}88 
public:: 
class:: 
TicketAttachmentDto::  
{;; 
public<< 

Guid<< 
Id<< 
{<< 
get<< 
;<< 
set<< 
;<< 
}<<  
public== 

string== 
FileName== 
{== 
get==  
;==  !
set==" %
;==% &
}==' (
===) *
string==+ 1
.==1 2
Empty==2 7
;==7 8
public>> 

string>> 
FileUrl>> 
{>> 
get>> 
;>>  
set>>! $
;>>$ %
}>>& '
=>>( )
string>>* 0
.>>0 1
Empty>>1 6
;>>6 7
public?? 

long?? 
FileSize?? 
{?? 
get?? 
;?? 
set??  #
;??# $
}??% &
public@@ 

string@@ 
ContentType@@ 
{@@ 
get@@  #
;@@# $
set@@% (
;@@( )
}@@* +
=@@, -
string@@. 4
.@@4 5
Empty@@5 :
;@@: ;
}AA 
publicCC 
classCC 
TicketResponseCC 
{DD 
publicEE 

GuidEE 
IdEE 
{EE 
getEE 
;EE 
setEE 
;EE 
}EE  
publicFF 

stringFF 
TitleFF 
{FF 
getFF 
;FF 
setFF "
;FF" #
}FF$ %
=FF& '
stringFF( .
.FF. /
EmptyFF/ 4
;FF4 5
publicGG 

stringGG 
DescriptionGG 
{GG 
getGG  #
;GG# $
setGG% (
;GG( )
}GG* +
=GG, -
stringGG. 4
.GG4 5
EmptyGG5 :
;GG: ;
publicHH 

intHH 

CategoryIdHH 
{HH 
getHH 
;HH  
setHH! $
;HH$ %
}HH& '
publicII 

stringII 
CategoryNameII 
{II  
getII! $
;II$ %
setII& )
;II) *
}II+ ,
=II- .
stringII/ 5
.II5 6
EmptyII6 ;
;II; <
publicJJ 

intJJ 
?JJ 
SubcategoryIdJJ 
{JJ 
getJJ  #
;JJ# $
setJJ% (
;JJ( )
}JJ* +
publicKK 

stringKK 
?KK 
SubcategoryNameKK "
{KK# $
getKK% (
;KK( )
setKK* -
;KK- .
}KK/ 0
publicLL 

TicketPriorityLL 
PriorityLL "
{LL# $
getLL% (
;LL( )
setLL* -
;LL- .
}LL/ 0
publicMM 

ListMM 
<MM &
TicketDynamicFieldResponseMM *
>MM* +
?MM+ ,
DynamicFieldsMM- :
{MM; <
getMM= @
;MM@ A
setMMB E
;MME F
}MMG H
publicNN 

ListNN 
<NN 
TicketAttachmentDtoNN #
>NN# $
?NN$ %
AttachmentsNN& 1
{NN2 3
getNN4 7
;NN7 8
setNN9 <
;NN< =
}NN> ?
publicOO 

TicketStatusOO 
CanonicalStatusOO '
{OO( )
getOO* -
;OO- .
setOO/ 2
;OO2 3
}OO4 5
publicPP 

TicketStatusPP 
DisplayStatusPP %
{PP& '
getPP( +
;PP+ ,
setPP- 0
;PP0 1
}PP2 3
[QQ 
ObsoleteQQ 
(QQ 
$strQQ O
)QQO P
]QQP Q
publicRR 

TicketStatusRR 
StatusRR 
=>RR !
DisplayStatusRR" /
;RR/ 0
publicSS 

GuidSS 
CreatedByUserIdSS 
{SS  !
getSS" %
;SS% &
setSS' *
;SS* +
}SS, -
publicTT 

stringTT 
CreatedByNameTT 
{TT  !
getTT" %
;TT% &
setTT' *
;TT* +
}TT, -
=TT. /
stringTT0 6
.TT6 7
EmptyTT7 <
;TT< =
publicUU 

stringUU 
CreatedByEmailUU  
{UU! "
getUU# &
;UU& '
setUU( +
;UU+ ,
}UU- .
=UU/ 0
stringUU1 7
.UU7 8
EmptyUU8 =
;UU= >
publicVV 

stringVV 
?VV  
CreatedByPhoneNumberVV '
{VV( )
getVV* -
;VV- .
setVV/ 2
;VV2 3
}VV4 5
publicWW 

stringWW 
?WW 
CreatedByDepartmentWW &
{WW' (
getWW) ,
;WW, -
setWW. 1
;WW1 2
}WW3 4
publicXX 

GuidXX 
?XX 
AssignedToUserIdXX !
{XX" #
getXX$ '
;XX' (
setXX) ,
;XX, -
}XX. /
publicYY 

stringYY 
?YY 
AssignedToNameYY !
{YY" #
getYY$ '
;YY' (
setYY) ,
;YY, -
}YY. /
publicZZ 

stringZZ 
?ZZ 
AssignedToEmailZZ "
{ZZ# $
getZZ% (
;ZZ( )
setZZ* -
;ZZ- .
}ZZ/ 0
public[[ 

string[[ 
?[[ !
AssignedToPhoneNumber[[ (
{[[) *
get[[+ .
;[[. /
set[[0 3
;[[3 4
}[[5 6
public\\ 

string\\ 
?\\ "
AssignedTechnicianName\\ )
{\\* +
get\\, /
;\\/ 0
set\\1 4
;\\4 5
}\\6 7
public`` 

List`` 
<`` 
TicketTechnicianDto`` #
>``# $
?``$ %
AssignedTechnicians``& 9
{``: ;
get``< ?
;``? @
set``A D
;``D E
}``F G
publicaa 

DateTimeaa 
	CreatedAtaa 
{aa 
getaa  #
;aa# $
setaa% (
;aa( )
}aa* +
publicbb 

DateTimebb 
?bb 
	UpdatedAtbb 
{bb  
getbb! $
;bb$ %
setbb& )
;bb) *
}bb+ ,
publiccc 

DateTimecc 
?cc 
DueDatecc 
{cc 
getcc "
;cc" #
setcc$ '
;cc' (
}cc) *
}dd 
publicff 
classff  
TicketMessageRequestff !
{gg 
publichh 

stringhh 
Messagehh 
{hh 
gethh 
;hh  
sethh! $
;hh$ %
}hh& '
=hh( )
stringhh* 0
.hh0 1
Emptyhh1 6
;hh6 7
publicii 

TicketStatusii 
?ii 
Statusii 
{ii  !
getii" %
;ii% &
setii' *
;ii* +
}ii, -
}jj 
publicll 
classll 
TicketMessageDtoll 
{mm 
publicnn 

Guidnn 
Idnn 
{nn 
getnn 
;nn 
setnn 
;nn 
}nn  
publicoo 

Guidoo 
AuthorUserIdoo 
{oo 
getoo "
;oo" #
setoo$ '
;oo' (
}oo) *
publicpp 

stringpp 

AuthorNamepp 
{pp 
getpp "
;pp" #
setpp$ '
;pp' (
}pp) *
=pp+ ,
stringpp- 3
.pp3 4
Emptypp4 9
;pp9 :
publicqq 

stringqq 
AuthorEmailqq 
{qq 
getqq  #
;qq# $
setqq% (
;qq( )
}qq* +
=qq, -
stringqq. 4
.qq4 5
Emptyqq5 :
;qq: ;
publicrr 

stringrr 
Messagerr 
{rr 
getrr 
;rr  
setrr! $
;rr$ %
}rr& '
=rr( )
stringrr* 0
.rr0 1
Emptyrr1 6
;rr6 7
publicss 

DateTimess 
	CreatedAtss 
{ss 
getss  #
;ss# $
setss% (
;ss( )
}ss* +
publictt 

TicketStatustt 
?tt 
Statustt 
{tt  !
gettt" %
;tt% &
settt' *
;tt* +
}tt, -
}uu 
publicww 
classww "
TicketCalendarResponseww #
{xx 
publicyy 

Guidyy 
Idyy 
{yy 
getyy 
;yy 
setyy 
;yy 
}yy  
publiczz 

stringzz 
TicketNumberzz 
{zz  
getzz! $
;zz$ %
setzz& )
;zz) *
}zz+ ,
=zz- .
stringzz/ 5
.zz5 6
Emptyzz6 ;
;zz; <
public{{ 

string{{ 
Title{{ 
{{{ 
get{{ 
;{{ 
set{{ "
;{{" #
}{{$ %
={{& '
string{{( .
.{{. /
Empty{{/ 4
;{{4 5
public|| 

TicketStatus|| 
CanonicalStatus|| '
{||( )
get||* -
;||- .
set||/ 2
;||2 3
}||4 5
public}} 

TicketStatus}} 
DisplayStatus}} %
{}}& '
get}}( +
;}}+ ,
set}}- 0
;}}0 1
}}}2 3
[~~ 
Obsolete~~ 
(~~ 
$str~~ 2
)~~2 3
]~~3 4
public 

TicketStatus 
Status 
=> !
DisplayStatus" /
;/ 0
public
ÄÄ 

TicketPriority
ÄÄ 
Priority
ÄÄ "
{
ÄÄ# $
get
ÄÄ% (
;
ÄÄ( )
set
ÄÄ* -
;
ÄÄ- .
}
ÄÄ/ 0
public
ÅÅ 

string
ÅÅ 
CategoryName
ÅÅ 
{
ÅÅ  
get
ÅÅ! $
;
ÅÅ$ %
set
ÅÅ& )
;
ÅÅ) *
}
ÅÅ+ ,
=
ÅÅ- .
string
ÅÅ/ 5
.
ÅÅ5 6
Empty
ÅÅ6 ;
;
ÅÅ; <
public
ÇÇ 

string
ÇÇ 
?
ÇÇ $
AssignedTechnicianName
ÇÇ )
{
ÇÇ* +
get
ÇÇ, /
;
ÇÇ/ 0
set
ÇÇ1 4
;
ÇÇ4 5
}
ÇÇ6 7
public
ÉÉ 

DateTime
ÉÉ 
	CreatedAt
ÉÉ 
{
ÉÉ 
get
ÉÉ  #
;
ÉÉ# $
set
ÉÉ% (
;
ÉÉ( )
}
ÉÉ* +
public
ÑÑ 

DateTime
ÑÑ 
?
ÑÑ 
DueDate
ÑÑ 
{
ÑÑ 
get
ÑÑ "
;
ÑÑ" #
set
ÑÑ$ '
;
ÑÑ' (
}
ÑÑ) *
}ÖÖ 
publicáá 
class
áá %
AssignTechnicianRequest
áá $
{àà 
public
ââ 

Guid
ââ 
TechnicianId
ââ 
{
ââ 
get
ââ "
;
ââ" #
set
ââ$ '
;
ââ' (
}
ââ) *
}ää 
publicåå 
class
åå &
AssignTechniciansRequest
åå %
{çç 
public
éé 

List
éé 
<
éé 
Guid
éé 
>
éé 
TechnicianIds
éé #
{
éé$ %
get
éé& )
;
éé) *
set
éé+ .
;
éé. /
}
éé0 1
=
éé2 3
new
éé4 7
(
éé7 8
)
éé8 9
;
éé9 :
public
èè 

Guid
èè 
?
èè 
LeadTechnicianId
èè !
{
èè" #
get
èè$ '
;
èè' (
set
èè) ,
;
èè, -
}
èè. /
}êê 
publicíí 
class
íí *
UpdateTechnicianStateRequest
íí )
{ìì 
public
îî 
#
TicketTechnicianState
îî  
State
îî! &
{
îî' (
get
îî) ,
;
îî, -
set
îî. 1
;
îî1 2
}
îî3 4
}ïï 
publicóó 
class
óó -
SetResponsibleTechnicianRequest
óó ,
{òò 
public
ôô 

Guid
ôô %
ResponsibleTechnicianId
ôô '
{
ôô( )
get
ôô* -
;
ôô- .
set
ôô/ 2
;
ôô2 3
}
ôô4 5
}öö 
publicúú 
class
úú "
HandoffTicketRequest
úú !
{ùù 
public
ûû 

Guid
ûû 
ToTechnicianId
ûû 
{
ûû  
get
ûû! $
;
ûû$ %
set
ûû& )
;
ûû) *
}
ûû+ ,
public
üü 

string
üü 
?
üü 
Reason
üü 
{
üü 
get
üü 
;
üü  
set
üü! $
;
üü$ %
}
üü& '
public
†† 

string
†† 
?
†† 
Note
†† 
{
†† 
get
†† 
;
†† 
set
†† "
;
††" #
}
††$ %
}°° 
public££ 
class
££ !
TicketTechnicianDto
££  
{§§ 
public
•• 

Guid
•• 
TechnicianId
•• 
{
•• 
get
•• "
;
••" #
set
••$ '
;
••' (
}
••) *
public
¶¶ 

Guid
¶¶ 
TechnicianUserId
¶¶  
{
¶¶! "
get
¶¶# &
;
¶¶& '
set
¶¶( +
;
¶¶+ ,
}
¶¶- .
public
ßß 

string
ßß 
TechnicianName
ßß  
{
ßß! "
get
ßß# &
;
ßß& '
set
ßß( +
;
ßß+ ,
}
ßß- .
=
ßß/ 0
string
ßß1 7
.
ßß7 8
Empty
ßß8 =
;
ßß= >
public
®® 

string
®® 
TechnicianEmail
®® !
{
®®" #
get
®®$ '
;
®®' (
set
®®) ,
;
®®, -
}
®®. /
=
®®0 1
string
®®2 8
.
®®8 9
Empty
®®9 >
;
®®> ?
public
©© 

bool
©© 
IsLead
©© 
{
©© 
get
©© 
;
©© 
set
©© !
;
©©! "
}
©©# $
public
™™ 
#
TicketTechnicianState
™™  
State
™™! &
{
™™' (
get
™™) ,
;
™™, -
set
™™. 1
;
™™1 2
}
™™3 4
public
´´ 

DateTime
´´ 

AssignedAt
´´ 
{
´´  
get
´´! $
;
´´$ %
set
´´& )
;
´´) *
}
´´+ ,
}¨¨ 
publicÆÆ 
class
ÆÆ 
TicketActivityDto
ÆÆ 
{ØØ 
public
∞∞ 

Guid
∞∞ 
Id
∞∞ 
{
∞∞ 
get
∞∞ 
;
∞∞ 
set
∞∞ 
;
∞∞ 
}
∞∞  
public
±± 

Guid
±± 
TicketId
±± 
{
±± 
get
±± 
;
±± 
set
±±  #
;
±±# $
}
±±% &
public
≤≤ 

Guid
≤≤ 
ActorUserId
≤≤ 
{
≤≤ 
get
≤≤ !
;
≤≤! "
set
≤≤# &
;
≤≤& '
}
≤≤( )
public
≥≥ 

string
≥≥ 
	ActorName
≥≥ 
{
≥≥ 
get
≥≥ !
;
≥≥! "
set
≥≥# &
;
≥≥& '
}
≥≥( )
=
≥≥* +
string
≥≥, 2
.
≥≥2 3
Empty
≥≥3 8
;
≥≥8 9
public
¥¥ 

string
¥¥ 

ActorEmail
¥¥ 
{
¥¥ 
get
¥¥ "
;
¥¥" #
set
¥¥$ '
;
¥¥' (
}
¥¥) *
=
¥¥+ ,
string
¥¥- 3
.
¥¥3 4
Empty
¥¥4 9
;
¥¥9 :
public
µµ 
 
TicketActivityType
µµ 
Type
µµ "
{
µµ# $
get
µµ% (
;
µµ( )
set
µµ* -
;
µµ- .
}
µµ/ 0
public
∂∂ 

string
∂∂ 
Message
∂∂ 
{
∂∂ 
get
∂∂ 
;
∂∂  
set
∂∂! $
;
∂∂$ %
}
∂∂& '
=
∂∂( )
string
∂∂* 0
.
∂∂0 1
Empty
∂∂1 6
;
∂∂6 7
public
∑∑ 

DateTime
∑∑ 
	CreatedAt
∑∑ 
{
∑∑ 
get
∑∑  #
;
∑∑# $
set
∑∑% (
;
∑∑( )
}
∑∑* +
}∏∏ 
public∫∫ 
class
∫∫ &
UpdateWorkSessionRequest
∫∫ %
{ªª 
public
ºº 

string
ºº 
	WorkingOn
ºº 
{
ºº 
get
ºº !
;
ºº! "
set
ºº# &
;
ºº& '
}
ºº( )
=
ºº* +
string
ºº, 2
.
ºº2 3
Empty
ºº3 8
;
ºº8 9
public
ΩΩ 

string
ΩΩ 
?
ΩΩ 
Note
ΩΩ 
{
ΩΩ 
get
ΩΩ 
;
ΩΩ 
set
ΩΩ "
;
ΩΩ" #
}
ΩΩ$ %
public
ææ 
#
TicketTechnicianState
ææ  
State
ææ! &
{
ææ' (
get
ææ) ,
;
ææ, -
set
ææ. 1
;
ææ1 2
}
ææ3 4
}øø 
public¡¡ 
class
¡¡ )
TicketCollaborationResponse
¡¡ (
{¬¬ 
public
√√ 

Guid
√√ 
TicketId
√√ 
{
√√ 
get
√√ 
;
√√ 
set
√√  #
;
√√# $
}
√√% &
public
ƒƒ 

TicketStatus
ƒƒ 
CanonicalStatus
ƒƒ '
{
ƒƒ( )
get
ƒƒ* -
;
ƒƒ- .
set
ƒƒ/ 2
;
ƒƒ2 3
}
ƒƒ4 5
public
≈≈ 

TicketStatus
≈≈ 
DisplayStatus
≈≈ %
{
≈≈& '
get
≈≈( +
;
≈≈+ ,
set
≈≈- 0
;
≈≈0 1
}
≈≈2 3
[
∆∆ 
Obsolete
∆∆ 
(
∆∆ 
$str
∆∆ 2
)
∆∆2 3
]
∆∆3 4
public
«« 

TicketStatus
«« 
Status
«« 
=>
«« !
DisplayStatus
««" /
;
««/ 0
public
»» 

TicketActivityDto
»» 
?
»» 
LastActivity
»» *
{
»»+ ,
get
»»- 0
;
»»0 1
set
»»2 5
;
»»5 6
}
»»7 8
public
…… 

List
…… 
<
…… 
TicketActivityDto
…… !
>
……! "
RecentActivities
……# 3
{
……4 5
get
……6 9
;
……9 :
set
……; >
;
……> ?
}
……@ A
=
……B C
new
……D G
(
……G H
)
……H I
;
……I J
public
   

List
   
<
   !
ActiveTechnicianDto
   #
>
  # $
ActiveTechnicians
  % 6
{
  7 8
get
  9 <
;
  < =
set
  > A
;
  A B
}
  C D
=
  E F
new
  G J
(
  J K
)
  K L
;
  L M
}ÀÀ 
publicÕÕ 
class
ÕÕ !
ActiveTechnicianDto
ÕÕ  
{ŒŒ 
public
œœ 

Guid
œœ 
TechnicianId
œœ 
{
œœ 
get
œœ "
;
œœ" #
set
œœ$ '
;
œœ' (
}
œœ) *
public
–– 

Guid
–– 
TechnicianUserId
––  
{
––! "
get
––# &
;
––& '
set
––( +
;
––+ ,
}
––- .
public
—— 

string
—— 
Name
—— 
{
—— 
get
—— 
;
—— 
set
—— !
;
——! "
}
——# $
=
——% &
string
——' -
.
——- .
Empty
——. 3
;
——3 4
public
““ 

string
““ 
	WorkingOn
““ 
{
““ 
get
““ !
;
““! "
set
““# &
;
““& '
}
““( )
=
““* +
string
““, 2
.
““2 3
Empty
““3 8
;
““8 9
public
”” 

string
”” 
?
”” 
Note
”” 
{
”” 
get
”” 
;
”” 
set
”” "
;
””" #
}
””$ %
public
‘‘ 
#
TicketTechnicianState
‘‘  
State
‘‘! &
{
‘‘' (
get
‘‘) ,
;
‘‘, -
set
‘‘. 1
;
‘‘1 2
}
‘‘3 4
public
’’ 

DateTime
’’ 
?
’’ 
	UpdatedAt
’’ 
{
’’  
get
’’! $
;
’’$ %
set
’’& )
;
’’) *
}
’’+ ,
}÷÷ 
publicÿÿ 
class
ÿÿ 
TicketSummaryDto
ÿÿ 
{ŸŸ 
public
⁄⁄ 

Guid
⁄⁄ 
Id
⁄⁄ 
{
⁄⁄ 
get
⁄⁄ 
;
⁄⁄ 
set
⁄⁄ 
;
⁄⁄ 
}
⁄⁄  
public
€€ 

string
€€ 
Title
€€ 
{
€€ 
get
€€ 
;
€€ 
set
€€ "
;
€€" #
}
€€$ %
=
€€& '
string
€€( .
.
€€. /
Empty
€€/ 4
;
€€4 5
public
‹‹ 

TicketStatus
‹‹ 
Status
‹‹ 
{
‹‹  
get
‹‹! $
;
‹‹$ %
set
‹‹& )
;
‹‹) *
}
‹‹+ ,
public
›› 

TicketPriority
›› 
Priority
›› "
{
››# $
get
››% (
;
››( )
set
››* -
;
››- .
}
››/ 0
public
ﬁﬁ 

DateTime
ﬁﬁ 
	CreatedAt
ﬁﬁ 
{
ﬁﬁ 
get
ﬁﬁ  #
;
ﬁﬁ# $
set
ﬁﬁ% (
;
ﬁﬁ( )
}
ﬁﬁ* +
public
ﬂﬂ 

DateTime
ﬂﬂ 
?
ﬂﬂ 
DueDate
ﬂﬂ 
{
ﬂﬂ 
get
ﬂﬂ "
;
ﬂﬂ" #
set
ﬂﬂ$ '
;
ﬂﬂ' (
}
ﬂﬂ) *
public
‡‡ 

string
‡‡ 
CategoryName
‡‡ 
{
‡‡  
get
‡‡! $
;
‡‡$ %
set
‡‡& )
;
‡‡) *
}
‡‡+ ,
=
‡‡- .
string
‡‡/ 5
.
‡‡5 6
Empty
‡‡6 ;
;
‡‡; <
public
·· 

string
·· 
?
·· 
SubcategoryName
·· "
{
··# $
get
··% (
;
··( )
set
··* -
;
··- .
}
··/ 0
public
‚‚ 

string
‚‚ 
CreatedByName
‚‚ 
{
‚‚  !
get
‚‚" %
;
‚‚% &
set
‚‚' *
;
‚‚* +
}
‚‚, -
=
‚‚. /
string
‚‚0 6
.
‚‚6 7
Empty
‚‚7 <
;
‚‚< =
public
„„ 

int
„„ 

CategoryId
„„ 
{
„„ 
get
„„ 
;
„„  
set
„„! $
;
„„$ %
}
„„& '
public
‰‰ 

int
‰‰ 
?
‰‰ 
SubcategoryId
‰‰ 
{
‰‰ 
get
‰‰  #
;
‰‰# $
set
‰‰% (
;
‰‰( )
}
‰‰* +
public
ÂÂ 

Guid
ÂÂ 
?
ÂÂ 
AssignedToUserId
ÂÂ !
{
ÂÂ" #
get
ÂÂ$ '
;
ÂÂ' (
set
ÂÂ) ,
;
ÂÂ, -
}
ÂÂ. /
public
ÊÊ 

string
ÊÊ 
?
ÊÊ $
AssignedTechnicianName
ÊÊ )
{
ÊÊ* +
get
ÊÊ, /
;
ÊÊ/ 0
set
ÊÊ1 4
;
ÊÊ4 5
}
ÊÊ6 7
}ÁÁ 
publicÈÈ 
class
ÈÈ (
TicketDynamicFieldResponse
ÈÈ '
{ÍÍ 
public
ÎÎ 

int
ÎÎ 
FieldDefinitionId
ÎÎ  
{
ÎÎ! "
get
ÎÎ# &
;
ÎÎ& '
set
ÎÎ( +
;
ÎÎ+ ,
}
ÎÎ- .
public
ÏÏ 

string
ÏÏ 
Key
ÏÏ 
{
ÏÏ 
get
ÏÏ 
;
ÏÏ 
set
ÏÏ  
;
ÏÏ  !
}
ÏÏ" #
=
ÏÏ$ %
string
ÏÏ& ,
.
ÏÏ, -
Empty
ÏÏ- 2
;
ÏÏ2 3
public
ÌÌ 

string
ÌÌ 
Label
ÌÌ 
{
ÌÌ 
get
ÌÌ 
;
ÌÌ 
set
ÌÌ "
;
ÌÌ" #
}
ÌÌ$ %
=
ÌÌ& '
string
ÌÌ( .
.
ÌÌ. /
Empty
ÌÌ/ 4
;
ÌÌ4 5
public
ÓÓ 

	FieldType
ÓÓ 
Type
ÓÓ 
{
ÓÓ 
get
ÓÓ 
;
ÓÓ  
set
ÓÓ! $
;
ÓÓ$ %
}
ÓÓ& '
public
ÔÔ 

string
ÔÔ 
Value
ÔÔ 
{
ÔÔ 
get
ÔÔ 
;
ÔÔ 
set
ÔÔ "
;
ÔÔ" #
}
ÔÔ$ %
=
ÔÔ& '
string
ÔÔ( .
.
ÔÔ. /
Empty
ÔÔ/ 4
;
ÔÔ4 5
public
 

bool
 

IsRequired
 
{
 
get
  
;
  !
set
" %
;
% &
}
' (
}ÒÒ 
publicÛÛ 
class
ÛÛ %
AssignmentQueueResponse
ÛÛ $
{ÙÙ 
public
ıı 

List
ıı 
<
ıı 
TicketSummaryDto
ıı  
>
ıı  !
?
ıı! "
Tickets
ıı# *
{
ıı+ ,
get
ıı- 0
;
ıı0 1
set
ıı2 5
;
ıı5 6
}
ıı7 8
public
ˆˆ 

List
ˆˆ 
<
ˆˆ 
TicketSummaryDto
ˆˆ  
>
ˆˆ  !
?
ˆˆ! "
Tasks
ˆˆ# (
{
ˆˆ) *
get
ˆˆ+ .
;
ˆˆ. /
set
ˆˆ0 3
;
ˆˆ3 4
}
ˆˆ5 6
public
˜˜ 

List
˜˜ 
<
˜˜ !
AssignmentQueueItem
˜˜ #
>
˜˜# $
Items
˜˜% *
{
˜˜+ ,
get
˜˜- 0
;
˜˜0 1
set
˜˜2 5
;
˜˜5 6
}
˜˜7 8
=
˜˜9 :
new
˜˜; >
(
˜˜> ?
)
˜˜? @
;
˜˜@ A
public
¯¯ 

int
¯¯ 

TotalCount
¯¯ 
{
¯¯ 
get
¯¯ 
;
¯¯  
set
¯¯! $
;
¯¯$ %
}
¯¯& '
}˘˘ 
public˚˚ 
class
˚˚ !
AssignmentQueueItem
˚˚  
{¸¸ 
public
˝˝ 

Guid
˝˝ 
Id
˝˝ 
{
˝˝ 
get
˝˝ 
;
˝˝ 
set
˝˝ 
;
˝˝ 
}
˝˝  
public
˛˛ 

string
˛˛ 
Title
˛˛ 
{
˛˛ 
get
˛˛ 
;
˛˛ 
set
˛˛ "
;
˛˛" #
}
˛˛$ %
=
˛˛& '
string
˛˛( .
.
˛˛. /
Empty
˛˛/ 4
;
˛˛4 5
public
ˇˇ 

TicketStatus
ˇˇ 
Status
ˇˇ 
{
ˇˇ  
get
ˇˇ! $
;
ˇˇ$ %
set
ˇˇ& )
;
ˇˇ) *
}
ˇˇ+ ,
public
ÄÄ 

TicketPriority
ÄÄ 
Priority
ÄÄ "
{
ÄÄ# $
get
ÄÄ% (
;
ÄÄ( )
set
ÄÄ* -
;
ÄÄ- .
}
ÄÄ/ 0
public
ÅÅ 

DateTime
ÅÅ 
	CreatedAt
ÅÅ 
{
ÅÅ 
get
ÅÅ  #
;
ÅÅ# $
set
ÅÅ% (
;
ÅÅ( )
}
ÅÅ* +
public
ÇÇ 

DateTime
ÇÇ 
?
ÇÇ 
DueDate
ÇÇ 
{
ÇÇ 
get
ÇÇ "
;
ÇÇ" #
set
ÇÇ$ '
;
ÇÇ' (
}
ÇÇ) *
public
ÉÉ 

string
ÉÉ 
CategoryName
ÉÉ 
{
ÉÉ  
get
ÉÉ! $
;
ÉÉ$ %
set
ÉÉ& )
;
ÉÉ) *
}
ÉÉ+ ,
=
ÉÉ- .
string
ÉÉ/ 5
.
ÉÉ5 6
Empty
ÉÉ6 ;
;
ÉÉ; <
public
ÑÑ 

string
ÑÑ 
?
ÑÑ 
SubcategoryName
ÑÑ "
{
ÑÑ# $
get
ÑÑ% (
;
ÑÑ( )
set
ÑÑ* -
;
ÑÑ- .
}
ÑÑ/ 0
public
ÖÖ 

string
ÖÖ 
CreatedByName
ÖÖ 
{
ÖÖ  !
get
ÖÖ" %
;
ÖÖ% &
set
ÖÖ' *
;
ÖÖ* +
}
ÖÖ, -
=
ÖÖ. /
string
ÖÖ0 6
.
ÖÖ6 7
Empty
ÖÖ7 <
;
ÖÖ< =
}ÜÜ Œ)
hC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\DTOs\TechnicianDtos.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
DTOs  $
;$ %
public 
class 
TechnicianResponse 
{ 
public 

Guid 
Id 
{ 
get 
; 
set 
; 
}  
public 

string 
FullName 
{ 
get  
;  !
set" %
;% &
}' (
=) *
string+ 1
.1 2
Empty2 7
;7 8
public 

string 
Email 
{ 
get 
; 
set "
;" #
}$ %
=& '
string( .
.. /
Empty/ 4
;4 5
public 

string 
? 
Phone 
{ 
get 
; 
set  #
;# $
}% &
public		 

string		 
?		 

Department		 
{		 
get		  #
;		# $
set		% (
;		( )
}		* +
public

 

bool

 
IsActive

 
{

 
get

 
;

 
set

  #
;

# $
}

% &
public 

DateTime 
	CreatedAt 
{ 
get  #
;# $
set% (
;( )
}* +
public 

Guid 
? 
UserId 
{ 
get 
; 
set "
;" #
}$ %
public 

List 
< 
int 
> 
SubcategoryIds #
{$ %
get& )
;) *
set+ .
;. /
}0 1
=2 3
new4 7
List8 <
<< =
int= @
>@ A
(A B
)B C
;C D
} 
public 
class #
TechnicianCreateRequest $
{ 
public 

string 
FullName 
{ 
get  
;  !
set" %
;% &
}' (
=) *
string+ 1
.1 2
Empty2 7
;7 8
public 

string 
Email 
{ 
get 
; 
set "
;" #
}$ %
=& '
string( .
.. /
Empty/ 4
;4 5
public 

string 
? 
Phone 
{ 
get 
; 
set  #
;# $
}% &
public 

string 
? 

Department 
{ 
get  #
;# $
set% (
;( )
}* +
public 

bool 
IsActive 
{ 
get 
; 
set  #
;# $
}% &
=' (
true) -
;- .
public   

List   
<   
int   
>   
?   
SubcategoryIds   $
{  % &
get  ' *
;  * +
set  , /
;  / 0
}  1 2
}!! 
public## 
class## #
TechnicianUpdateRequest## $
{$$ 
public%% 

string%% 
FullName%% 
{%% 
get%%  
;%%  !
set%%" %
;%%% &
}%%' (
=%%) *
string%%+ 1
.%%1 2
Empty%%2 7
;%%7 8
public&& 

string&& 
Email&& 
{&& 
get&& 
;&& 
set&& "
;&&" #
}&&$ %
=&&& '
string&&( .
.&&. /
Empty&&/ 4
;&&4 5
public'' 

string'' 
?'' 
Phone'' 
{'' 
get'' 
;'' 
set''  #
;''# $
}''% &
public(( 

string(( 
?(( 

Department(( 
{(( 
get((  #
;((# $
set((% (
;((( )
}((* +
public)) 

bool)) 
IsActive)) 
{)) 
get)) 
;)) 
set))  #
;))# $
}))% &
=))' (
true))) -
;))- .
public-- 

List-- 
<-- 
int-- 
>-- 
?-- 
SubcategoryIds-- $
{--% &
get--' *
;--* +
set--, /
;--/ 0
}--1 2
}.. 
public00 
class00 )
TechnicianStatusUpdateRequest00 *
{11 
public22 

bool22 
IsActive22 
{22 
get22 
;22 
set22  #
;22# $
}22% &
}33 
public88 
class88 %
TechnicianLinkUserRequest88 &
{99 
public== 

Guid== 
UserId== 
{== 
get== 
;== 
set== !
;==! "
}==# $
}>> Û
sC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\Exceptions\BadRequestException.cs
	namespace 	
	Ticketing
 
. 
Application 
.  

Exceptions  *
;* +
public 
class 
BadRequestException  
:! "
	Exception# ,
{ 
public 

BadRequestException 
( 
string %
message& -
)- .
:/ 0
base1 5
(5 6
message6 =
)= >
{? @
}A B
} Úb
lC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\DTOs\SystemSettingsDtos.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
DTOs  $
;$ %
public 
class "
SystemSettingsResponse #
{ 
public		 

string		 
AppName		 
{		 
get		 
;		  
set		! $
;		$ %
}		& '
=		( )
string		* 0
.		0 1
Empty		1 6
;		6 7
public

 

string

 
SupportEmail

 
{

  
get

! $
;

$ %
set

& )
;

) *
}

+ ,
=

- .
string

/ 5
.

5 6
Empty

6 ;
;

; <
public 

string 
SupportPhone 
{  
get! $
;$ %
set& )
;) *
}+ ,
=- .
string/ 5
.5 6
Empty6 ;
;; <
public 

string 
DefaultLanguage !
{" #
get$ '
;' (
set) ,
;, -
}. /
=0 1
$str2 6
;6 7
public 

string 
DefaultTheme 
{  
get! $
;$ %
set& )
;) *
}+ ,
=- .
$str/ 7
;7 8
public 

string 
Timezone 
{ 
get  
;  !
set" %
;% &
}' (
=) *
$str+ 8
;8 9
public 

TicketPriority 
DefaultPriority )
{* +
get, /
;/ 0
set1 4
;4 5
}6 7
=8 9
TicketPriority: H
.H I
MediumI O
;O P
public 

TicketStatus 
DefaultStatus %
{& '
get( +
;+ ,
set- 0
;0 1
}2 3
=4 5
TicketStatus6 B
.B C
	SubmittedC L
;L M
public 

int 
ResponseSlaHours 
{  !
get" %
;% &
set' *
;* +
}, -
=. /
$num0 2
;2 3
public 

bool 
AutoAssignEnabled !
{" #
get$ '
;' (
set) ,
;, -
}. /
=0 1
false2 7
;7 8
public 

bool "
AllowClientAttachments &
{' (
get) ,
;, -
set. 1
;1 2
}3 4
=5 6
true7 ;
;; <
public 

int 
MaxAttachmentSizeMB "
{# $
get% (
;( )
set* -
;- .
}/ 0
=1 2
$num3 5
;5 6
public 

bool %
EmailNotificationsEnabled )
{* +
get, /
;/ 0
set1 4
;4 5
}6 7
=8 9
true: >
;> ?
public 

bool #
SmsNotificationsEnabled '
{( )
get* -
;- .
set/ 2
;2 3
}4 5
=6 7
false8 =
;= >
public 

bool !
NotifyOnTicketCreated %
{& '
get( +
;+ ,
set- 0
;0 1
}2 3
=4 5
true6 :
;: ;
public 

bool "
NotifyOnTicketAssigned &
{' (
get) ,
;, -
set. 1
;1 2
}3 4
=5 6
true7 ;
;; <
public 

bool !
NotifyOnTicketReplied %
{& '
get( +
;+ ,
set- 0
;0 1
}2 3
=4 5
true6 :
;: ;
public 

bool  
NotifyOnTicketClosed $
{% &
get' *
;* +
set, /
;/ 0
}1 2
=3 4
true5 9
;9 :
public!! 

int!! 
PasswordMinLength!!  
{!!! "
get!!# &
;!!& '
set!!( +
;!!+ ,
}!!- .
=!!/ 0
$num!!1 2
;!!2 3
public"" 

bool"" 

Require2FA"" 
{"" 
get""  
;""  !
set""" %
;""% &
}""' (
="") *
false""+ 0
;""0 1
public## 

int## !
SessionTimeoutMinutes## $
{##% &
get##' *
;##* +
set##, /
;##/ 0
}##1 2
=##3 4
$num##5 7
;##7 8
public$$ 

List$$ 
<$$ 
string$$ 
>$$ 
AllowedEmailDomains$$ +
{$$, -
get$$. 1
;$$1 2
set$$3 6
;$$6 7
}$$8 9
=$$: ;
new$$< ?
($$? @
)$$@ A
;$$A B
}%% 
public'' 
class'' '
SystemSettingsUpdateRequest'' (
{(( 
[** 
Required** 
(** 
ErrorMessage** 
=** 
$str** 4
)**4 5
]**5 6
[++ 
StringLength++ 
(++ 
$num++ 
,++ 
ErrorMessage++ #
=++$ %
$str++& V
)++V W
]++W X
public,, 

string,, 
AppName,, 
{,, 
get,, 
;,,  
set,,! $
;,,$ %
},,& '
=,,( )
string,,* 0
.,,0 1
Empty,,1 6
;,,6 7
[.. 
Required.. 
(.. 
ErrorMessage.. 
=.. 
$str.. 8
)..8 9
]..9 :
[// 
EmailAddress// 
(// 
ErrorMessage// 
=//  
$str//! 8
)//8 9
]//9 :
public00 

string00 
SupportEmail00 
{00  
get00! $
;00$ %
set00& )
;00) *
}00+ ,
=00- .
string00/ 5
.005 6
Empty006 ;
;00; <
[22 
StringLength22 
(22 
$num22 
,22 
ErrorMessage22 "
=22# $
$str22% T
)22T U
]22U V
public33 

string33 
SupportPhone33 
{33  
get33! $
;33$ %
set33& )
;33) *
}33+ ,
=33- .
string33/ 5
.335 6
Empty336 ;
;33; <
[55 
Required55 
]55 
[66 
RegularExpression66 
(66 
$str66 "
,66" #
ErrorMessage66$ 0
=661 2
$str663 L
)66L M
]66M N
public77 

string77 
DefaultLanguage77 !
{77" #
get77$ '
;77' (
set77) ,
;77, -
}77. /
=770 1
$str772 6
;776 7
[99 
Required99 
]99 
[:: 
RegularExpression:: 
(:: 
$str:: .
,::. /
ErrorMessage::0 <
=::= >
$str::? c
)::c d
]::d e
public;; 

string;; 
DefaultTheme;; 
{;;  
get;;! $
;;;$ %
set;;& )
;;;) *
};;+ ,
=;;- .
$str;;/ 7
;;;7 8
[== 
Required== 
]== 
[>> 
StringLength>> 
(>> 
$num>> 
)>> 
]>> 
public?? 

string?? 
Timezone?? 
{?? 
get??  
;??  !
set??" %
;??% &
}??' (
=??) *
$str??+ 8
;??8 9
[BB 
RequiredBB 
]BB 
publicCC 

TicketPriorityCC 
DefaultPriorityCC )
{CC* +
getCC, /
;CC/ 0
setCC1 4
;CC4 5
}CC6 7
=CC8 9
TicketPriorityCC: H
.CCH I
MediumCCI O
;CCO P
[EE 
RequiredEE 
]EE 
publicFF 

TicketStatusFF 
DefaultStatusFF %
{FF& '
getFF( +
;FF+ ,
setFF- 0
;FF0 1
}FF2 3
=FF4 5
TicketStatusFF6 B
.FFB C
	SubmittedFFC L
;FFL M
[HH 
RangeHH 

(HH
 
$numHH 
,HH 
$numHH 
,HH 
ErrorMessageHH 
=HH  !
$strHH" H
)HHH I
]HHI J
publicII 

intII 
ResponseSlaHoursII 
{II  !
getII" %
;II% &
setII' *
;II* +
}II, -
=II. /
$numII0 2
;II2 3
publicKK 

boolKK 
AutoAssignEnabledKK !
{KK" #
getKK$ '
;KK' (
setKK) ,
;KK, -
}KK. /
=KK0 1
falseKK2 7
;KK7 8
publicLL 

boolLL "
AllowClientAttachmentsLL &
{LL' (
getLL) ,
;LL, -
setLL. 1
;LL1 2
}LL3 4
=LL5 6
trueLL7 ;
;LL; <
[NN 
RangeNN 

(NN
 
$numNN 
,NN 
$numNN 
,NN 
ErrorMessageNN 
=NN  !
$strNN" R
)NNR S
]NNS T
publicOO 

intOO 
MaxAttachmentSizeMBOO "
{OO# $
getOO% (
;OO( )
setOO* -
;OO- .
}OO/ 0
=OO1 2
$numOO3 5
;OO5 6
publicRR 

boolRR %
EmailNotificationsEnabledRR )
{RR* +
getRR, /
;RR/ 0
setRR1 4
;RR4 5
}RR6 7
=RR8 9
trueRR: >
;RR> ?
publicSS 

boolSS #
SmsNotificationsEnabledSS '
{SS( )
getSS* -
;SS- .
setSS/ 2
;SS2 3
}SS4 5
=SS6 7
falseSS8 =
;SS= >
publicTT 

boolTT !
NotifyOnTicketCreatedTT %
{TT& '
getTT( +
;TT+ ,
setTT- 0
;TT0 1
}TT2 3
=TT4 5
trueTT6 :
;TT: ;
publicUU 

boolUU "
NotifyOnTicketAssignedUU &
{UU' (
getUU) ,
;UU, -
setUU. 1
;UU1 2
}UU3 4
=UU5 6
trueUU7 ;
;UU; <
publicVV 

boolVV !
NotifyOnTicketRepliedVV %
{VV& '
getVV( +
;VV+ ,
setVV- 0
;VV0 1
}VV2 3
=VV4 5
trueVV6 :
;VV: ;
publicWW 

boolWW  
NotifyOnTicketClosedWW $
{WW% &
getWW' *
;WW* +
setWW, /
;WW/ 0
}WW1 2
=WW3 4
trueWW5 9
;WW9 :
[ZZ 
RangeZZ 

(ZZ
 
$numZZ 
,ZZ 
$numZZ 
,ZZ 
ErrorMessageZZ 
=ZZ  
$strZZ! S
)ZZS T
]ZZT U
public[[ 

int[[ 
PasswordMinLength[[  
{[[! "
get[[# &
;[[& '
set[[( +
;[[+ ,
}[[- .
=[[/ 0
$num[[1 2
;[[2 3
public]] 

bool]] 

Require2FA]] 
{]] 
get]]  
;]]  !
set]]" %
;]]% &
}]]' (
=]]) *
false]]+ 0
;]]0 1
[__ 
Range__ 

(__
 
$num__ 
,__ 
$num__ 
,__ 
ErrorMessage__  
=__! "
$str__# S
)__S T
]__T U
public`` 

int`` !
SessionTimeoutMinutes`` $
{``% &
get``' *
;``* +
set``, /
;``/ 0
}``1 2
=``3 4
$num``5 7
;``7 8
publicbb 

Listbb 
<bb 
stringbb 
>bb 
AllowedEmailDomainsbb +
{bb, -
getbb. 1
;bb1 2
setbb3 6
;bb6 7
}bb8 9
=bb: ;
newbb< ?
(bb? @
)bb@ A
;bbA B
}cc ‘
mC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\DTOs\SmartAssignmentDtos.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
DTOs  $
;$ %
public 
class )
SmartAssignmentStatusResponse *
{ 
public 

bool 
Enabled 
{ 
get 
; 
set "
;" #
}$ %
} 
public 
class (
SmartAssignmentUpdateRequest )
{		 
public

 

bool

 
Enabled

 
{

 
get

 
;

 
set

 "
;

" #
}

$ %
} 
public 
class %
SmartAssignmentRunRequest &
{ 
public 

DateTime 
? 
	StartDate 
{  
get! $
;$ %
set& )
;) *
}+ ,
public 

DateTime 
? 
EndDate 
{ 
get "
;" #
set$ '
;' (
}) *
public 

string 
? 
Scope 
{ 
get 
; 
set  #
;# $
}% &
} 
public 
class &
SmartAssignmentRunResponse '
{ 
public 

int 
AssignedCount 
{ 
get "
;" #
set$ '
;' (
}) *
public 

string 
Message 
{ 
get 
;  
set! $
;$ %
}& '
=( )
string* 0
.0 1
Empty1 6
;6 7
} »&
fC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\DTOs\CategoryDtos.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
DTOs  $
;$ %
public 
class 
CategoryRequest 
{ 
public 

string 
Name 
{ 
get 
; 
set !
;! "
}# $
=% &
string' -
.- .
Empty. 3
;3 4
public 

string 
? 
Description 
{  
get! $
;$ %
set& )
;) *
}+ ,
public 

bool 
IsActive 
{ 
get 
; 
set  #
;# $
}% &
=' (
true) -
;- .
} 
public

 
class

 
CategoryResponse

 
{ 
public 

int 
Id 
{ 
get 
; 
set 
; 
} 
public 

string 
Name 
{ 
get 
; 
set !
;! "
}# $
=% &
string' -
.- .
Empty. 3
;3 4
public 

string 
? 
Description 
{  
get! $
;$ %
set& )
;) *
}+ ,
public 

bool 
IsActive 
{ 
get 
; 
set  #
;# $
}% &
public 

DateTime 
	CreatedAt 
{ 
get  #
;# $
set% (
;( )
}* +
public 

IEnumerable 
< 
SubcategoryResponse *
>* +
Subcategories, 9
{: ;
get< ?
;? @
setA D
;D E
}F G
=H I

EnumerableJ T
.T U
EmptyU Z
<Z [
SubcategoryResponse[ n
>n o
(o p
)p q
;q r
} 
public 
class 
SubcategoryRequest 
{ 
public 

string 
Name 
{ 
get 
; 
set !
;! "
}# $
=% &
string' -
.- .
Empty. 3
;3 4
public 

string 
? 
Description 
{  
get! $
;$ %
set& )
;) *
}+ ,
public 

bool 
IsActive 
{ 
get 
; 
set  #
;# $
}% &
=' (
true) -
;- .
} 
public 
class 
SubcategoryResponse  
{ 
public 

int 
Id 
{ 
get 
; 
set 
; 
} 
public 

string 
Name 
{ 
get 
; 
set !
;! "
}# $
=% &
string' -
.- .
Empty. 3
;3 4
public 

string 
? 
Description 
{  
get! $
;$ %
set& )
;) *
}+ ,
public   

bool   
IsActive   
{   
get   
;   
set    #
;  # $
}  % &
public!! 

DateTime!! 
	CreatedAt!! 
{!! 
get!!  #
;!!# $
set!!% (
;!!( )
}!!* +
}"" 
public$$ 
class$$  
CategoryListResponse$$ !
{%% 
public&& 

IEnumerable&& 
<&& 
CategoryResponse&& '
>&&' (
Items&&) .
{&&/ 0
get&&1 4
;&&4 5
set&&6 9
;&&9 :
}&&; <
=&&= >

Enumerable&&? I
.&&I J
Empty&&J O
<&&O P
CategoryResponse&&P `
>&&` a
(&&a b
)&&b c
;&&c d
public'' 

int'' 

TotalCount'' 
{'' 
get'' 
;''  
set''! $
;''$ %
}''& '
public(( 

int(( 
Page(( 
{(( 
get(( 
;(( 
set(( 
;(( 
}((  !
public)) 

int)) 
PageSize)) 
{)) 
get)) 
;)) 
set)) "
;))" #
}))$ %
}** £9
bC:\Users\user\Desktop\42\TikQ\backend\Ticketing.Backend\src\Ticketing.Application\DTOs\AuthDtos.cs
	namespace 	
	Ticketing
 
. 
Application 
.  
DTOs  $
;$ %
public 
class 
RegisterRequest 
{ 
[ 
Required 
( 
ErrorMessage 
= 
$str 3
)3 4
]4 5
public 

string 
FullName 
{ 
get  
;  !
set" %
;% &
}' (
=) *
string+ 1
.1 2
Empty2 7
;7 8
[ 
Required 
( 
ErrorMessage 
= 
$str 0
)0 1
]1 2
[ 
EmailAddress 
( 
ErrorMessage 
=  
$str! 7
)7 8
]8 9
public 

string 
Email 
{ 
get 
; 
set "
;" #
}$ %
=& '
string( .
.. /
Empty/ 4
;4 5
[ 
Required 
( 
ErrorMessage 
= 
$str 3
)3 4
]4 5
[ 
	MinLength 
( 
$num 
, 
ErrorMessage 
=  
$str! I
)I J
]J K
public 

string 
Password 
{ 
get  
;  !
set" %
;% &
}' (
=) *
string+ 1
.1 2
Empty2 7
;7 8
[## 
Required## 
(## 
ErrorMessage## 
=## 
$str	## Ö
)
##Ö Ü
]
##Ü á
public$$ 

UserRole$$ 
?$$ 
Role$$ 
{$$ 
get$$ 
;$$  
set$$! $
;$$$ %
}$$& '
public&& 

string&& 
?&& 
PhoneNumber&& 
{&&  
get&&! $
;&&$ %
set&&& )
;&&) *
}&&+ ,
public'' 

string'' 
?'' 

Department'' 
{'' 
get''  #
;''# $
set''% (
;''( )
}''* +
}(( 
public** 
record** 
LoginRequest** 
(** 
string** !
Email**" '
,**' (
string**) /
Password**0 8
)**8 9
;**9 :
public,, 
class,, !
ChangePasswordRequest,, "
{-- 
[.. 
Required.. 
(.. 
ErrorMessage.. 
=.. 
$str.. 7
)..7 8
]..8 9
public// 

string// 
CurrentPassword// !
{//" #
get//$ '
;//' (
set//) ,
;//, -
}//. /
=//0 1
string//2 8
.//8 9
Empty//9 >
;//> ?
[11 
Required11 
(11 
ErrorMessage11 
=11 
$str11 7
)117 8
]118 9
[22 
	MinLength22 
(22 
$num22 
,22 
ErrorMessage22 
=22  
$str22! J
)22J K
]22K L
[33 
RegularExpression33 
(33 
$str33 4
,334 5
ErrorMessage336 B
=33C D
$str33E y
)33y z
]33z {
public44 

string44 
NewPassword44 
{44 
get44  #
;44# $
set44% (
;44( )
}44* +
=44, -
string44. 4
.444 5
Empty445 :
;44: ;
[66 
Required66 
(66 
ErrorMessage66 
=66 
$str66 8
)668 9
]669 :
[77 
Compare77 
(77 
nameof77 
(77 
NewPassword77 
)77  
,77  !
ErrorMessage77" .
=77/ 0
$str771 Y
)77Y Z
]77Z [
public88 

string88 
ConfirmNewPassword88 $
{88% &
get88' *
;88* +
set88, /
;88/ 0
}881 2
=883 4
string885 ;
.88; <
Empty88< A
;88A B
}99 
public;; 
class;;  
UpdateProfileRequest;; !
{<< 
public== 

string== 
?== 
FullName== 
{== 
get== !
;==! "
set==# &
;==& '
}==( )
public>> 

string>> 
?>> 
Email>> 
{>> 
get>> 
;>> 
set>>  #
;>># $
}>>% &
public?? 

string?? 
??? 
PhoneNumber?? 
{??  
get??! $
;??$ %
set??& )
;??) *
}??+ ,
public@@ 

string@@ 
?@@ 

Department@@ 
{@@ 
get@@  #
;@@# $
set@@% (
;@@( )
}@@* +
publicAA 

stringAA 
?AA 
	AvatarUrlAA 
{AA 
getAA "
;AA" #
setAA$ '
;AA' (
}AA) *
}BB 
publicDD 
classDD 
AuthResponseDD 
{EE 
publicFF 

stringFF 
TokenFF 
{FF 
getFF 
;FF 
setFF "
;FF" #
}FF$ %
=FF& '
stringFF( .
.FF. /
EmptyFF/ 4
;FF4 5
publicGG 

UserDtoGG 
?GG 
UserGG 
{GG 
getGG 
;GG 
setGG  #
;GG# $
}GG% &
}HH 
publicJJ 
classJJ 
UserDtoJJ 
{KK 
publicLL 

GuidLL 
IdLL 
{LL 
getLL 
;LL 
setLL 
;LL 
}LL  
publicMM 

stringMM 
FullNameMM 
{MM 
getMM  
;MM  !
setMM" %
;MM% &
}MM' (
=MM) *
stringMM+ 1
.MM1 2
EmptyMM2 7
;MM7 8
publicNN 

stringNN 
EmailNN 
{NN 
getNN 
;NN 
setNN "
;NN" #
}NN$ %
=NN& '
stringNN( .
.NN. /
EmptyNN/ 4
;NN4 5
publicOO 

UserRoleOO 
RoleOO 
{OO 
getOO 
;OO 
setOO  #
;OO# $
}OO% &
publicPP 

stringPP 
?PP 
PhoneNumberPP 
{PP  
getPP! $
;PP$ %
setPP& )
;PP) *
}PP+ ,
publicQQ 

stringQQ 
?QQ 

DepartmentQQ 
{QQ 
getQQ  #
;QQ# $
setQQ% (
;QQ( )
}QQ* +
publicRR 

stringRR 
?RR 
	AvatarUrlRR 
{RR 
getRR "
;RR" #
setRR$ '
;RR' (
}RR) *
}SS 