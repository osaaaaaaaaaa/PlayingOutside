use realtime_game;

show tables;
SELECT *
FROM users
WHERE id = 1
   OR id = 6;

# name指定でユーザーを取得
SELECT ut.id, ut.name, ut.character_id, rating
FROM users AS ut
         LEFT JOIN ratings ON ratings.id = ut.id
where ut.name = 'ブラックひよこ';

# 相互フォローしているユーザー一覧取得
SELECT users.id
FROM users
         LEFT JOIN follows ON followee_id = users.id
WHERE follows.followee_id IN ((SELECT following_id FROM follows WHERE followee_id = 3))
  AND follows.following_id = 3
ORDER BY users.id;

# フォロー一覧取得
SELECT ut.id, ut.name, ut.character_id, rating
FROM users
         LEFT JOIN follows ON users.id = follows.following_id
         LEFT JOIN users AS ut ON ut.id = follows.followee_id
         LEFT JOIN ratings ON ratings.id = ut.id
where following_id = 1
ORDER BY ut.id;

# [全ユーザー対象]レーティングランキング取得
SELECT ut.id, ut.name, ut.character_id, rt.rating
FROM users AS ut
         LEFT JOIN ratings AS rt ON ut.id = rt.user_id
ORDER BY rt.rating DESC;

# [フォローしているユーザーのみ対象]レーティングランキング取得
SELECT ut.id, ut.name, ut.character_id, rt.rating
FROM ratings AS rt
         JOIN users AS ut ON rt.user_id = ut.id
         JOIN (SELECT * FROM follows WHERE following_id = 1) AS ft ON ut.id = ft.followee_id
ORDER BY rt.rating DESC;

# 自分のランキング取得
SELECT ut.id, ut.name, ut.character_id, rt.rating
FROM users AS ut
         LEFT JOIN ratings AS rt ON ut.id = rt.user_id
WHERE ut.id = 1
ORDER BY rt.rating DESC;



