
use admin_console;

show tables;
SELECT * FROM stage_results where user_id = 21;
SELECT * FROM personal_access_tokens;
UPDATE users SET stage_id = 30 WHERE id = 27;
UPDATE items SET name = 'よく蹴られる' WHERE id = 39;

# 自身が発信していない && 自身が参加していない && ゲストの参加人数が2人未満の救難信号をランダムに10件まで取得
SELECT d_signals.id AS d_signal_id,d_signals.user_id, stage_id, action, IFNULL(cnt,0) AS cnt_guest, d_signals.created_at from distress_signals AS d_signals
LEFT JOIN guests AS guest1 ON distress_signal_id = d_signals.id
LEFT JOIN (SELECT COUNT(*) AS cnt, distress_signal_id FROM guests GROUP BY distress_signal_id) AS guest2 ON guest2.distress_signal_id = d_signals.id
where d_signals.user_id != 2 && d_signals.action = 0 && guest1.user_id != 2
            || d_signals.user_id != 2 && d_signals.action = 0 && guest1.user_id IS NULL
HAVING cnt_guest < 2;


# ゲストの場合の救難信号ログ取得処理
SELECT d_signals.id AS d_signal_id, stage_id, action, IFNULL(cnt,0) AS cnt_guest, guests.is_rewarded, d_signals.created_at FROM guests
INNER JOIN distress_signals AS d_signals ON guests.distress_signal_id = d_signals.id
LEFT JOIN (SELECT COUNT(*) AS cnt, distress_signal_id FROM guests GROUP BY distress_signal_id) AS sub_guest
ON d_signals.id = sub_guest.distress_signal_id
where guests.user_id = 1;

# アチーブメントの取得
SELECT achievements.id AS id,title,text,type,achieved_val,IFNULL(progress_val,0) AS progress_val,IFNULL(is_achieved,0) AS is_achieved FROM achievements
LEFT JOIN user_achievements AS ua ON achievements.id = ua.achievement_id
AND ua.user_id = 2;

# typeとuser_idを指定してまだ達成していないアチーブメントを取得する更新
SELECT achievements.id AS id, IFNULL(progress_val,0) AS progress_val, IFNULL(is_achieved,0) AS is_achieved FROM achievements
LEFT JOIN user_achievements AS ua ON achievements.id = ua.achievement_id
AND type = 1 AND user_id = 2 AND is_achieved = 0
ORDER BY id;

# typeとuser_idを指定してまだ達成していないアチーブメントを取得する
SELECT achievements.id AS id, achieved_val, IFNULL(progress_val,''), is_achieved AS is_arrive FROM achievements
LEFT JOIN user_achievements ON user_achievements.achievement_id = achievements.id
AND user_id = 2
WHERE type = 1 AND (is_achieved = 0 OR is_achieved IS NULL);

# アチーブメントマスタ取得処理
SELECT * from achievements;

explain SELECT achievements.id AS id, progress_val FROM achievements
LEFT JOIN user_achievements AS ua ON achievements.id = ua.achievement_id
AND type = 1 AND user_id = 2 AND ua.is_achieved = 0;

# [ランキング取得] ########################################################################

# 全ユーザーから検索して、100件まで取得する
SELECT DENSE_RANK() OVER (ORDER BY SUM(score) DESC ) AS rank_no,user_id,name,achievement_id,SUM(score) AS total from stage_results
INNER JOIN users ON stage_results.user_id = users.id
GROUP BY stage_results.user_id LIMIT 100;

# フォローしているユーザーから、100件まで取得する
# フォローしているユーザー + 自身を取得
SELECT user_id,name,achievement_id,SUM(score) AS total from stage_results
INNER JOIN users ON stage_results.user_id = users.id
WHERE user_id = 1
GROUP BY stage_results.user_id LIMIT 999;

SELECT DENSE_RANK() OVER (ORDER BY SUM(score) DESC ) AS rank_no,sr.user_id,name,achievement_id,SUM(score) AS total from stage_results AS sr
INNER JOIN users ON sr.user_id = users.id
INNER JOIN following_users AS fu ON users.id = fu.following_user_id
WHERE fu.user_id = 1
GROUP BY sr.user_id LIMIT 999;

# フォローしているユーザー取得
SELECT * FROM users
INNER JOIN following_users ON users.id = following_users.following_user_id
where following_users.user_id = 1;


# [ おすすめのユーザーの取得 ] ##################################################

# 自身をフォローしているユーザー
SELECT * FROM following_users where following_user_id = 2;

# 自分がフォローしているユーザーを取得
SELECT * FROM following_users WHERE user_id = 1;
