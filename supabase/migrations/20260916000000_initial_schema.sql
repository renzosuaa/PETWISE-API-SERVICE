-- Name: PetWise; Type: SCHEMA; Schema: -; Owner: postgres

--

-- Drop existing objects cleanly to prevent conflicts
DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;
DROP SCHEMA IF EXISTS "PetWise" CASCADE;

CREATE SCHEMA "PetWise";

ALTER SCHEMA "PetWise" OWNER TO postgres;

--

-- Name: get_pet_activity_and_health_stats(integer); Type: FUNCTION; Schema: PetWise; Owner: postgres

--

CREATE FUNCTION "PetWise".get_pet_activity_and_health_stats(target_pet_id integer) RETURNS json

    LANGUAGE plpgsql

    AS $$

DECLARE

    result JSON;

BEGIN

    SELECT json_build_object(

        'totalScheduledActivities', COALESCE((SELECT COUNT(*)::INT FROM "PetWise"."Activity" WHERE pet_id = target_pet_id), 0),

        'totalActiveRoutines', COALESCE((SELECT COUNT(*)::INT FROM "PetWise"."Activity" WHERE pet_id = target_pet_id AND is_active = true), 0),

        -- Count only health events for the current month

        'totalHealthEvents', COALESCE((SELECT COUNT(*)::INT FROM "PetWise"."HealthEvent" WHERE pet_id = target_pet_id AND DATE_TRUNC('month', event_date) = DATE_TRUNC('month', CURRENT_DATE)), 0),

        'medicalComplianceRate', COALESCE(

            ROUND(

                (COUNT(CASE WHEN is_completed = true THEN 1 END)::NUMERIC / NULLIF(COUNT(*), 0)::NUMERIC) * 100, 2

            ), 100.0

        ),

        'activityRecurrenceDistribution', COALESCE(

            (SELECT json_object_agg(COALESCE(recurrence, 'None'), cnt)

             FROM (SELECT recurrence, COUNT(*)::INT as cnt FROM "PetWise"."Activity" WHERE pet_id = target_pet_id GROUP BY recurrence) sub), '{}'::json

        ),

        -- Chart Distribution: Active, uncompleted health events scheduled for this month

        'healthEventTypeDistribution', COALESCE(

            (SELECT json_object_agg(COALESCE(type, 'Other'), cnt)

             FROM (

                 SELECT type, COUNT(*)::INT as cnt 

                 FROM "PetWise"."HealthEvent" 

                 WHERE pet_id = target_pet_id 

                   AND is_completed = false 

                   AND DATE_TRUNC('month', event_date) = DATE_TRUNC('month', CURRENT_DATE)

                 GROUP BY type

             ) sub), '{}'::json

        ),

        'activityTimeline', COALESCE(

            (SELECT json_agg(json_build_object('timeSlot', slot, 'count', cnt))

             FROM (

                 SELECT TO_CHAR(time_scheduled, 'HH24:00') AS slot, COUNT(*)::INT as cnt 

                 FROM "PetWise"."Activity" 

                 WHERE pet_id = target_pet_id AND time_scheduled IS NOT NULL

                 GROUP BY slot 

                 ORDER BY slot

             ) sub), '[]'::json

        )

    ) INTO result

    FROM "PetWise"."HealthEvent"

    WHERE pet_id = target_pet_id 

      AND DATE_TRUNC('month', event_date) = DATE_TRUNC('month', CURRENT_DATE);

    -- Fallback handling if the pet has activities but no health events recorded this month

    IF result IS NULL OR (result->>'totalScheduledActivities')::INT = 0 AND (result->>'totalHealthEvents')::INT = 0 THEN

        SELECT json_build_object(

            'totalScheduledActivities', COALESCE((SELECT COUNT(*)::INT FROM "PetWise"."Activity" WHERE pet_id = target_pet_id), 0),

            'totalActiveRoutines', COALESCE((SELECT COUNT(*)::INT FROM "PetWise"."Activity" WHERE pet_id = target_pet_id AND is_active = true), 0),

            'totalHealthEvents', 0,

            'medicalComplianceRate', 100.0,

            'activityRecurrenceDistribution', COALESCE((SELECT json_object_agg(COALESCE(recurrence, 'None'), cnt) FROM (SELECT recurrence, COUNT(*)::INT as cnt FROM "PetWise"."Activity" WHERE pet_id = target_pet_id GROUP BY recurrence) s), '{}'::json),

            'healthEventTypeDistribution', '{}'::json,

            'activityTimeline', COALESCE((SELECT json_agg(json_build_object('timeSlot', slot, 'count', cnt)) FROM (SELECT TO_CHAR(time_scheduled, 'HH24:00') AS slot, COUNT(*)::INT as cnt FROM "PetWise"."Activity" WHERE pet_id = target_pet_id AND time_scheduled IS NOT NULL GROUP BY slot ORDER BY slot) s), '[]'::json)

        ) INTO result;

    END IF;

    RETURN result;

END;

$$;

ALTER FUNCTION "PetWise".get_pet_activity_and_health_stats(target_pet_id integer) OWNER TO postgres;

--

-- Name: get_user_all_pets_stats(uuid); Type: FUNCTION; Schema: PetWise; Owner: postgres

--

CREATE FUNCTION "PetWise".get_user_all_pets_stats(target_user_id uuid) RETURNS json

    LANGUAGE plpgsql

    AS $$

DECLARE

    result JSON;

BEGIN

    SELECT json_build_object(

        'totalPets', COALESCE((SELECT COUNT(*)::INT FROM "PetWise"."Pet" WHERE user_id = target_user_id AND is_deleted = false), 0),

        'totalScheduledActivities', COALESCE((SELECT COUNT(*)::INT FROM "PetWise"."Activity" a JOIN "PetWise"."Pet" p ON a.pet_id = p.pet_id WHERE p.user_id = target_user_id AND p.is_deleted = false), 0),

        'totalActiveRoutines', COALESCE((SELECT COUNT(*)::INT FROM "PetWise"."Activity" a JOIN "PetWise"."Pet" p ON a.pet_id = p.pet_id WHERE p.user_id = target_user_id AND a.is_active = true AND p.is_deleted = false), 0),

        -- Count only global health events for the current month

        'totalHealthEvents', COALESCE((SELECT COUNT(*)::INT FROM "PetWise"."HealthEvent" e JOIN "PetWise"."Pet" p ON e.pet_id = p.pet_id WHERE p.user_id = target_user_id AND p.is_deleted = false AND DATE_TRUNC('month', e.event_date) = DATE_TRUNC('month', CURRENT_DATE)), 0),

        'medicalComplianceRate', COALESCE(

            ROUND(

                (COUNT(CASE WHEN e.is_completed = true THEN 1 END)::NUMERIC / NULLIF(COUNT(*), 0)::NUMERIC) * 100, 2

            ), 100.0

        ),

        'activityRecurrenceDistribution', COALESCE(

            (SELECT json_object_agg(COALESCE(recurrence, 'None'), cnt)

             FROM (SELECT a.recurrence, COUNT(*)::INT as cnt FROM "PetWise"."Activity" a JOIN "PetWise"."Pet" p ON a.pet_id = p.pet_id WHERE p.user_id = target_user_id AND p.is_deleted = false GROUP BY a.recurrence) sub), '{}'::json

        ),

        -- Global User Chart: Count only uncompleted health events scheduled for this calendar month

        'healthEventTypeDistribution', COALESCE(

            (SELECT json_object_agg(COALESCE(type, 'Other'), cnt)

             FROM (

                 SELECT e.type, COUNT(*)::INT as cnt 

                 FROM "PetWise"."HealthEvent" e 

                 JOIN "PetWise"."Pet" p ON e.pet_id = p.pet_id 

                 WHERE p.user_id = target_user_id 

                   AND p.is_deleted = false 

                   AND e.is_completed = false 

                   AND DATE_TRUNC('month', e.event_date) = DATE_TRUNC('month', CURRENT_DATE)

                 GROUP BY e.type

             ) sub), '{}'::json

        ),

        'activityTimeline', COALESCE(

            (SELECT json_agg(json_build_object('timeSlot', slot, 'count', cnt))

             FROM (

                 SELECT TO_CHAR(a.time_scheduled, 'HH24:00') AS slot, COUNT(*)::INT as cnt 

                 FROM "PetWise"."Activity" a

                 JOIN "PetWise"."Pet" p ON a.pet_id = p.pet_id

                 WHERE p.user_id = target_user_id AND p.is_deleted = false AND a.time_scheduled IS NOT NULL

                 GROUP BY slot 

                 ORDER BY slot

             ) sub), '[]'::json

        )

    ) INTO result

    FROM "PetWise"."HealthEvent" e

    JOIN "PetWise"."Pet" p ON e.pet_id = p.pet_id

    WHERE p.user_id = target_user_id 

      AND p.is_deleted = false

      AND DATE_TRUNC('month', e.event_date) = DATE_TRUNC('month', CURRENT_DATE);

    -- Fallback handling if the user has pets but zero health events logged this month

    IF result IS NULL OR (result->>'totalPets')::INT = 0 THEN

        SELECT json_build_object(

            'totalPets', COALESCE((SELECT COUNT(*)::INT FROM "PetWise"."Pet" WHERE user_id = target_user_id AND is_deleted = false), 0),

            'totalScheduledActivities', COALESCE((SELECT COUNT(*)::INT FROM "PetWise"."Activity" a JOIN "PetWise"."Pet" p ON a.pet_id = p.pet_id WHERE p.user_id = target_user_id AND p.is_deleted = false), 0),

            'totalActiveRoutines', COALESCE((SELECT COUNT(*)::INT FROM "PetWise"."Activity" a JOIN "PetWise"."Pet" p ON a.pet_id = p.pet_id WHERE p.user_id = target_user_id AND a.is_active = true AND p.is_deleted = false), 0),

            'totalHealthEvents', 0,

            'medicalComplianceRate', 100.0,

            'activityRecurrenceDistribution', COALESCE((SELECT json_object_agg(COALESCE(recurrence, 'None'), cnt) FROM (SELECT a.recurrence, COUNT(*)::INT as cnt FROM "PetWise"."Activity" a JOIN "PetWise"."Pet" p ON a.pet_id = p.pet_id WHERE p.user_id = target_user_id AND p.is_deleted = false GROUP BY a.recurrence) s), '{}'::json),

            'healthEventTypeDistribution', '{}'::json,

            'activityTimeline', COALESCE((SELECT json_agg(json_build_object('timeSlot', slot, 'count', cnt)) FROM (SELECT TO_CHAR(a.time_scheduled, 'HH24:00') AS slot, COUNT(*)::INT as cnt FROM "PetWise"."Activity" a JOIN "PetWise"."Pet" p ON a.pet_id = p.pet_id WHERE p.user_id = target_user_id AND p.is_deleted = false AND a.time_scheduled IS NOT NULL GROUP BY slot ORDER BY slot) s), '[]'::json)

        ) INTO result;

    END IF;

    RETURN result;

END;

$$;

ALTER FUNCTION "PetWise".get_user_all_pets_stats(target_user_id uuid) OWNER TO postgres;

--

-- Name: handle_auth_user_created(); Type: FUNCTION; Schema: PetWise; Owner: postgres

--

CREATE FUNCTION "PetWise".handle_auth_user_created() RETURNS trigger

    LANGUAGE plpgsql SECURITY DEFINER

    AS $$

BEGIN

  INSERT INTO "PetWise"."User" (user_id, first_name, last_name, email, created_at)

  VALUES (NEW.id, NULL, NULL, NEW.email, NOW())

  ON CONFLICT (user_id) DO NOTHING;

  RETURN NEW;

END;

$$;

ALTER FUNCTION "PetWise".handle_auth_user_created() OWNER TO postgres;

--

-- Name: handle_new_user(); Type: FUNCTION; Schema: PetWise; Owner: postgres

--

CREATE FUNCTION "PetWise".handle_new_user() RETURNS trigger

    LANGUAGE plpgsql SECURITY DEFINER

    AS $$

BEGIN

  INSERT INTO "PetWise"."User" (user_id, email, created_at)

  VALUES (NEW.id::uuid, NEW.email, NOW());

  RETURN NEW;

END;

$$;

ALTER FUNCTION "PetWise".handle_new_user() OWNER TO postgres;

--

-- Name: Activity; Type: TABLE; Schema: PetWise; Owner: postgres

--

CREATE TABLE "PetWise"."Activity" (

    activity_id bigint NOT NULL,

    created_at timestamp with time zone DEFAULT now() NOT NULL,

    pet_id bigint NOT NULL,

    title character varying NOT NULL,

    description character varying,

    time_scheduled time without time zone NOT NULL,

    is_active boolean DEFAULT false NOT NULL,

    recurrence character varying DEFAULT '"None"'::character varying

);

ALTER TABLE "PetWise"."Activity" OWNER TO postgres;

--

-- Name: Activity_activity_id_seq; Type: SEQUENCE; Schema: PetWise; Owner: postgres

--

ALTER TABLE "PetWise"."Activity" ALTER COLUMN activity_id ADD GENERATED BY DEFAULT AS IDENTITY (

    SEQUENCE NAME "PetWise"."Activity_activity_id_seq"

    START WITH 1

    INCREMENT BY 1

    NO MINVALUE

    NO MAXVALUE

    CACHE 1

);

--

-- Name: HealthEvent; Type: TABLE; Schema: PetWise; Owner: postgres

--

CREATE TABLE "PetWise"."HealthEvent" (

    event_id bigint NOT NULL,

    created_at timestamp with time zone DEFAULT now() NOT NULL,

    event_name character varying NOT NULL,

    event_date timestamp with time zone NOT NULL,

    type character varying NOT NULL,

    is_completed boolean DEFAULT false NOT NULL,

    pet_id bigint NOT NULL

);

ALTER TABLE "PetWise"."HealthEvent" OWNER TO postgres;

--

-- Name: HealthEvent_event_id_seq; Type: SEQUENCE; Schema: PetWise; Owner: postgres

--

ALTER TABLE "PetWise"."HealthEvent" ALTER COLUMN event_id ADD GENERATED BY DEFAULT AS IDENTITY (

    SEQUENCE NAME "PetWise"."HealthEvent_event_id_seq"

    START WITH 1

    INCREMENT BY 1

    NO MINVALUE

    NO MAXVALUE

    CACHE 1

);

--

-- Name: Pet; Type: TABLE; Schema: PetWise; Owner: postgres

--

CREATE TABLE "PetWise"."Pet" (

    pet_id bigint NOT NULL,

    created_at timestamp with time zone DEFAULT now() NOT NULL,

    name character varying NOT NULL,

    species character varying NOT NULL,

    birthday date NOT NULL,

    sex character varying,

    user_id uuid DEFAULT gen_random_uuid() NOT NULL,

    weight real,

    breed character varying,

    is_deleted boolean DEFAULT false NOT NULL,

    image_url character varying

);

ALTER TABLE "PetWise"."Pet" OWNER TO postgres;

--

-- Name: Pet_pet_id_seq; Type: SEQUENCE; Schema: PetWise; Owner: postgres

--

ALTER TABLE "PetWise"."Pet" ALTER COLUMN pet_id ADD GENERATED BY DEFAULT AS IDENTITY (

    SEQUENCE NAME "PetWise"."Pet_pet_id_seq"

    START WITH 1

    INCREMENT BY 1

    NO MINVALUE

    NO MAXVALUE

    CACHE 1

);

--

-- Name: User; Type: TABLE; Schema: PetWise; Owner: postgres

--

CREATE TABLE "PetWise"."User" (

    user_id uuid DEFAULT gen_random_uuid() NOT NULL,

    created_at timestamp with time zone DEFAULT now() NOT NULL,

    email character varying NOT NULL,

    first_name character varying,

    last_name character varying,

    nickname character varying,

    image_url character varying,

    has_completed_setup boolean DEFAULT false NOT NULL

);

ALTER TABLE "PetWise"."User" OWNER TO postgres;

--

-- Name: COLUMN "User".has_completed_setup; Type: COMMENT; Schema: PetWise; Owner: postgres

--

COMMENT ON COLUMN "PetWise"."User".has_completed_setup IS 'to know whether the user acc is newly created or not';

--

-- Name: Activity Activity_pkey; Type: CONSTRAINT; Schema: PetWise; Owner: postgres

--

ALTER TABLE ONLY "PetWise"."Activity"

    ADD CONSTRAINT "Activity_pkey" PRIMARY KEY (activity_id);

--

-- Name: HealthEvent HealthEvent_pkey; Type: CONSTRAINT; Schema: PetWise; Owner: postgres

--

ALTER TABLE ONLY "PetWise"."HealthEvent"

    ADD CONSTRAINT "HealthEvent_pkey" PRIMARY KEY (event_id);

--

-- Name: Pet Pet_pkey; Type: CONSTRAINT; Schema: PetWise; Owner: postgres

--

ALTER TABLE ONLY "PetWise"."Pet"

    ADD CONSTRAINT "Pet_pkey" PRIMARY KEY (pet_id);

--

-- Name: User User_email_key; Type: CONSTRAINT; Schema: PetWise; Owner: postgres

--

ALTER TABLE ONLY "PetWise"."User"

    ADD CONSTRAINT "User_email_key" UNIQUE (email);

--

-- Name: User User_pkey; Type: CONSTRAINT; Schema: PetWise; Owner: postgres

--

ALTER TABLE ONLY "PetWise"."User"

    ADD CONSTRAINT "User_pkey" PRIMARY KEY (user_id);

--

-- Name: users on_auth_user_created; Type: TRIGGER; Schema: auth; Owner: supabase_auth_admin

--

CREATE TRIGGER on_auth_user_created AFTER INSERT ON auth.users FOR EACH ROW EXECUTE FUNCTION "PetWise".handle_new_user();

--

-- Name: Activity Activity_pet_id_fkey; Type: FK CONSTRAINT; Schema: PetWise; Owner: postgres

--

ALTER TABLE ONLY "PetWise"."Activity"

    ADD CONSTRAINT "Activity_pet_id_fkey" FOREIGN KEY (pet_id) REFERENCES "PetWise"."Pet"(pet_id);

--

-- Name: HealthEvent HealthEvent_pet_id_fkey; Type: FK CONSTRAINT; Schema: PetWise; Owner: postgres

--

ALTER TABLE ONLY "PetWise"."HealthEvent"

    ADD CONSTRAINT "HealthEvent_pet_id_fkey" FOREIGN KEY (pet_id) REFERENCES "PetWise"."Pet"(pet_id);

--

-- Name: Pet Pet_user_id_fkey; Type: FK CONSTRAINT; Schema: PetWise; Owner: postgres

--

ALTER TABLE ONLY "PetWise"."Pet"

    ADD CONSTRAINT "Pet_user_id_fkey" FOREIGN KEY (user_id) REFERENCES "PetWise"."User"(user_id);

--

-- Name: User Allow All; Type: POLICY; Schema: PetWise; Owner: postgres

--

CREATE POLICY "Allow All" ON "PetWise"."User" USING (true) WITH CHECK (true);

--

-- Name: User Allow delete own; Type: POLICY; Schema: PetWise; Owner: postgres

--

CREATE POLICY "Allow delete own" ON "PetWise"."User" FOR DELETE TO authenticated USING ((auth.uid() = user_id));

--

-- Name: User Allow insert own; Type: POLICY; Schema: PetWise; Owner: postgres

--

CREATE POLICY "Allow insert own" ON "PetWise"."User" FOR INSERT TO authenticated WITH CHECK ((auth.uid() = user_id));

--

-- Name: User Allow select own; Type: POLICY; Schema: PetWise; Owner: postgres

--

CREATE POLICY "Allow select own" ON "PetWise"."User" FOR SELECT TO authenticated USING ((auth.uid() = user_id));

--

-- Name: User Allow update own; Type: POLICY; Schema: PetWise; Owner: postgres

--

CREATE POLICY "Allow update own" ON "PetWise"."User" FOR UPDATE TO authenticated USING ((auth.uid() = user_id)) WITH CHECK ((auth.uid() = user_id));

--

-- Name: User; Type: ROW SECURITY; Schema: PetWise; Owner: postgres

--

ALTER TABLE "PetWise"."User" ENABLE ROW LEVEL SECURITY;

--

-- Name: SCHEMA "PetWise"; Type: ACL; Schema: -; Owner: postgres

--

GRANT USAGE ON SCHEMA "PetWise" TO anon;

GRANT USAGE ON SCHEMA "PetWise" TO authenticated;

GRANT USAGE ON SCHEMA "PetWise" TO service_role;

--

-- Name: TABLE "Activity"; Type: ACL; Schema: PetWise; Owner: postgres

--

GRANT ALL ON TABLE "PetWise"."Activity" TO anon;

GRANT ALL ON TABLE "PetWise"."Activity" TO authenticated;

GRANT ALL ON TABLE "PetWise"."Activity" TO service_role;

--

-- Name: SEQUENCE "Activity_activity_id_seq"; Type: ACL; Schema: PetWise; Owner: postgres

--

GRANT SELECT,USAGE ON SEQUENCE "PetWise"."Activity_activity_id_seq" TO anon;

GRANT SELECT,USAGE ON SEQUENCE "PetWise"."Activity_activity_id_seq" TO authenticated;

--

-- Name: TABLE "HealthEvent"; Type: ACL; Schema: PetWise; Owner: postgres

--

GRANT ALL ON TABLE "PetWise"."HealthEvent" TO anon;

GRANT ALL ON TABLE "PetWise"."HealthEvent" TO authenticated;

GRANT ALL ON TABLE "PetWise"."HealthEvent" TO service_role;

--

-- Name: SEQUENCE "HealthEvent_event_id_seq"; Type: ACL; Schema: PetWise; Owner: postgres

--

GRANT SELECT,USAGE ON SEQUENCE "PetWise"."HealthEvent_event_id_seq" TO anon;

GRANT SELECT,USAGE ON SEQUENCE "PetWise"."HealthEvent_event_id_seq" TO authenticated;

--

-- Name: TABLE "Pet"; Type: ACL; Schema: PetWise; Owner: postgres

--

GRANT ALL ON TABLE "PetWise"."Pet" TO anon;

GRANT ALL ON TABLE "PetWise"."Pet" TO authenticated;

GRANT ALL ON TABLE "PetWise"."Pet" TO service_role;

--

-- Name: SEQUENCE "Pet_pet_id_seq"; Type: ACL; Schema: PetWise; Owner: postgres

--

GRANT SELECT,USAGE ON SEQUENCE "PetWise"."Pet_pet_id_seq" TO anon;

GRANT SELECT,USAGE ON SEQUENCE "PetWise"."Pet_pet_id_seq" TO authenticated;

--

-- Name: TABLE "User"; Type: ACL; Schema: PetWise; Owner: postgres

--

GRANT ALL ON TABLE "PetWise"."User" TO authenticated;

GRANT ALL ON TABLE "PetWise"."User" TO anon;

GRANT ALL ON TABLE "PetWise"."User" TO service_role;

-- Reload Supabase API cache
NOTIFY pgrst, 'reload schema';